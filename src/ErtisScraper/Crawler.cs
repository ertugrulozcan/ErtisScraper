using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ErtisScraper.Abstractions;
using ErtisScraper.Extensions;
using HtmlAgilityPack;
using PuppeteerSharp;

namespace ErtisScraper
{
	public sealed class Crawler : ICrawler
	{
		#region Properties

		private IBrowserContext BrowserContext { get; set; }

		public CrawlerTarget Target { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="target"></param>
		internal Crawler(CrawlerTarget target)
		{
			this.Target = target;
		}

		#endregion

		#region Public Methods

		internal async Task<Crawler> InitializeAsync()
		{
			this.BrowserContext = await ErtisScraper.BrowserContext.CreateAsync();
			return this;
		}

		public async Task<ScrapingResult<T>> ScrapeAsync<T>(string url) where T : class
		{
			var result = await this.ScrapeAsync(url);
			var json = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data);
			var data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
			return new ScrapingResult<T>
			{
				Data = data,
				Errors = result.Errors,
				ElapsedMilliseconds = result.ElapsedMilliseconds
			};
		}

		public async Task<ScrapingResult> ScrapeAsync(string url)
		{
			var stopwatch = Stopwatch.StartNew();
			var htmlDocument = new HtmlDocument();

			try
			{
				var html = await this.FetchHtmlAsync(url, this.Target.Options?.WaitFor);
				htmlDocument.LoadHtml(html);
			}
			catch (Exception ex)
			{
				stopwatch.Stop();
				return new ScrapingResult
				{
					Data = null,
					Errors = new[] { ex.Message },
					ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
				};
			}
			
			var result = GetObject(htmlDocument.DocumentNode, this.Target.Schema, out string[] errors);
			stopwatch.Stop();
			
			return new ScrapingResult
			{
				Data = result,
				Errors = errors,
				ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
			};
		}

		#endregion

		#region Private Methods

		private static dynamic GetObject(HtmlNode rootNode, IEnumerable<FieldInfo> schema, out string[] errors)
		{
			var dictionary = new Dictionary<string, object>();
			var errorList = new List<string>();

			foreach (var fieldInfo in schema)
			{
				var value = GetFieldValue(rootNode, fieldInfo, out var innerErrors);
				if (value != null)
				{
					dictionary.Add(fieldInfo.Name, value);
				}

				if (innerErrors != null)
				{
					errorList.AddRange(innerErrors);
				}
			}

			errors = errorList.Any() ? errorList.ToArray() : null;
			if (dictionary.Any())
			{
				return dictionary.ConvertToDynamicObject();
			}
			else
			{
				return null;
			}
		}

		private static object GetFieldValue(HtmlNode rootNode, FieldInfo fieldInfo, out string[] errors)
		{
			errors = null;
			if (rootNode == null || fieldInfo == null)
			{
				return null;
			}
			
			try
			{
				var formatter = new FieldFormatter(fieldInfo.Options.Format);
				if (fieldInfo.Type.IsArray)
				{
					var arrayValues = new List<object>();

					var parentNode = !string.IsNullOrEmpty(fieldInfo.XPath) ? 
						rootNode.SelectNodes(fieldInfo.XPath).FirstOrDefault() : 
						rootNode.FindNode(fieldInfo.Route);

					var errorList = new List<string>();
					if (parentNode != null)
					{
						foreach (var node in parentNode.ChildNodes)
						{
							if (node.Name == "#text")
							{
								continue;
							}
							
							var childValue = GetFieldValue(node, fieldInfo.Enumerator, out var innerErrors);
							if (childValue != null)
							{
								arrayValues.Add(childValue);	
							}
						
							if (innerErrors != null)
							{
								errorList.AddRange(innerErrors);
							}
						}
					}
					else
					{
						errorList.Add($"Parent node could not find for '{fieldInfo.Name}'");
					}

					errors = errorList.Any() ? errorList.ToArray() : null;
					return arrayValues;
				}

				if (fieldInfo.Type.IsObject)
				{
					if (fieldInfo.ObjectSchema == null || fieldInfo.ObjectSchema.Length == 0)
					{
						throw new Exception($"Field type declared as object for '{fieldInfo.Name}' but schema missing!");
					}

					return GetObject(rootNode, fieldInfo.ObjectSchema, out errors);
				}
				else
				{
					HtmlNode node;
					if (!string.IsNullOrEmpty(fieldInfo.XPath))
					{
						var selectedNodes = rootNode.SelectNodes(fieldInfo.XPath);
						if (selectedNodes == null || !selectedNodes.Any())
						{
							node = rootNode.ChildNodes.FirstOrDefault(x => x.XPath == rootNode.XPath + fieldInfo.XPath);
						}
						else
						{
							node = selectedNodes.FirstOrDefault();	
						}
					}
					else
					{
						node = rootNode.FindNode(fieldInfo.Route);
					}

					if (node != null)
					{
						if (!string.IsNullOrEmpty(fieldInfo.AttributeName))
						{
							return node.GetAttribute(fieldInfo.AttributeName, fieldInfo.Type.BaseType, formatter);
						}
						else
						{
							return node.GetInnerText(fieldInfo.Type.BaseType, formatter);
						}
					}
					else
					{
						return null;
					}
				}
			}
			catch (Exception ex)
			{
				errors = new[] { ex.Message };
				return null;
			}
		}

		private async Task<string> FetchHtmlAsync(string url, WaitForOptions waitForOptions = null)
		{
			try
			{
				await using var page = await this.BrowserContext.Browser.NewPageAsync();
				await this.FixUserAgent(page);

				if (this.Target.Options?.Viewport != null)
				{
					await page.SetViewportAsync(new ViewPortOptions
					{
						Height = this.Target.Options.Viewport.Height, 
						Width = this.Target.Options.Viewport.Width, 
						HasTouch = this.Target.Options.Viewport.HasTouch, 
						IsLandscape = this.Target.Options.Viewport.IsLandscape, 
						IsMobile = this.Target.Options.Viewport.IsMobile, 
						DeviceScaleFactor = this.Target.Options.Viewport.ScaleFactor
					});	
				}

				await page.GoToAsync(url);

				if (waitForOptions != null)
				{
					if (waitForOptions.Selector.StartsWith(XPathSelector.XPathSelectorToken))
					{
						await page.WaitForXPathAsync(waitForOptions.Selector, new WaitForSelectorOptions
						{
							Hidden = waitForOptions.Hidden,
							Visible = waitForOptions.Visible,
							Timeout = waitForOptions.TimeOut
						});	
					}
					else
					{
						await page.WaitForSelectorAsync(waitForOptions.Selector, new WaitForSelectorOptions
						{
							Hidden = waitForOptions.Hidden,
							Visible = waitForOptions.Visible,
							Timeout = waitForOptions.TimeOut
						});	
					}
				}
				
				if (this.Target.Interactions != null && this.Target.Interactions.Any())
				{
					foreach (var interactionFunction in this.Target.Interactions)
					{
						await interactionFunction.ExecuteAsync(page).ConfigureAwait(false);
					}
				}
				
				return await page.GetContentAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}

		private async Task FixUserAgent(Page page)
		{
			var userAgent = await this.BrowserContext.Browser.GetUserAgentAsync();
			await page.SetUserAgentAsync(userAgent.Replace("Headless", string.Empty));
		}

		#endregion

		#region Disposing

		public void Dispose()
		{
			this.Dispose(disposing: true);
			// ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
			GC.SuppressFinalize(this);
		}

		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore();
			this.Dispose(disposing: false);

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
			// ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
			GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.BrowserContext?.Dispose();
			}

			this.Target = null;
		}

		private async ValueTask DisposeAsyncCore()
		{
			await this.BrowserContext.DisposeAsync().ConfigureAwait(false);
			this.Target = null;
		}

		#endregion
	}
}