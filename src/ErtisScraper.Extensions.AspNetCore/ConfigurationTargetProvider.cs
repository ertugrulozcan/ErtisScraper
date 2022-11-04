using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ErtisScraper.Abstractions;
using ErtisScraper.Interactions;
using Microsoft.Extensions.Configuration;

namespace ErtisScraper.Extensions.AspNetCore
{
	public class ConfigurationTargetProvider : ITargetProvider
	{
		#region Services

		private readonly IConfiguration configuration;

		#endregion
		
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="configuration"></param>
		public ConfigurationTargetProvider(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		#endregion
		
		#region Methods

		public IEnumerable<CrawlerTarget> GetTargets()
		{
			const string targetsSectionKey = "Targets";

			var targetsSection = configuration.GetSection(targetsSectionKey);
			var targetSections = targetsSection.GetChildren();
			foreach (var targetSection in targetSections)
			{
				var crawlerTarget = targetSection.Get<CrawlerTarget>();
				crawlerTarget.Schema = ParseSchema(targetSection);
				crawlerTarget.Interactions = ParseInteractions(targetSection);

				yield return crawlerTarget;
			}
		}
		
		public CrawlerTarget GetTarget(string name)
		{
			return this.GetTargets().FirstOrDefault(x => x.Name == name);
		}

		private static IEnumerable<FieldInfo> ParseSchema(IConfiguration targetSection)
		{
			var fieldInfos = new List<FieldInfo>();
			var fieldSections = targetSection.GetSection("schema").GetChildren();
			foreach (var fieldSection in fieldSections)
			{
				var fieldInfo = ParseToFieldInfo(fieldSection);
				if (fieldInfo != null)
				{
					fieldInfos.Add(fieldInfo);	
				}
			}

			return fieldInfos;
		}

		private static FieldInfo ParseToFieldInfo([NotNull] IConfigurationSection fieldSection)
		{
			if (fieldSection == null)
			{
				return null;
			}
			
			var fieldName = fieldSection.Key;
			var fieldDescription = fieldSection.GetValue<string>("description");
			var fieldTypeName = fieldSection.GetValue<string>("type");
			var fieldRoute = fieldSection.GetSection("route").Get<string[]>();
			var fieldXPath = fieldSection.GetValue<string>("xpath");
			var fieldAttributeName = fieldSection.GetValue<string>("attribute");
			var fieldOptions = fieldSection.GetSection("options").Get<FieldOptions>();
			var fieldArrayEnumeratorSection = fieldTypeName == "array" ? fieldSection.GetSection("enumerator") : null;

			if (string.IsNullOrEmpty(fieldName))
			{
				throw new Exception("Field name is required!");
			}
			
			if (!FieldType.TryParse(fieldTypeName, out var fieldType))
			{
				throw new Exception($"Field type missing or unsupported for '{fieldName}'");	
			}
			
			FieldInfo fieldEnumeratorInfo = null;
			if (fieldArrayEnumeratorSection != null)
			{
				fieldEnumeratorInfo = ParseToFieldInfo(fieldArrayEnumeratorSection);
			}

			FieldInfo[] fieldObjectSchema = default;
			if (fieldType.IsObject)
			{
				var objectSchemaSection = fieldSection.GetSection("schema");
				var objectSchemaFieldInfos = new List<FieldInfo>();
				var objectSchemaFieldSections = objectSchemaSection.GetChildren();
				foreach (var objectSchemaFieldSection in objectSchemaFieldSections)
				{
					objectSchemaFieldInfos.Add(ParseToFieldInfo(objectSchemaFieldSection));
				}

				fieldObjectSchema = objectSchemaFieldInfos.ToArray();
			}
				
			var fieldInfo = new FieldInfo
			{
				Name = fieldName,
				Description = fieldDescription,
				Route = fieldRoute,
				XPath = fieldXPath,
				Type = fieldType,
				AttributeName = fieldAttributeName,
				Options = fieldOptions,
				Enumerator = fieldEnumeratorInfo,
				ObjectSchema = fieldObjectSchema
			};

			return fieldInfo;
		}

		private static IEnumerable<IInteractionFunction> ParseInteractions(IConfiguration targetSection)
		{
			var interactions = new List<IInteractionFunction>();
			var interactionSections = targetSection.GetSection("interactions").GetChildren();
			foreach (var interactionSection in interactionSections)
			{
				var interaction = ParseToInteraction(interactionSection);
				if (interaction != null)
				{
					interactions.Add(interaction);	
				}
			}

			return interactions;
		}

		private static IInteractionFunction ParseToInteraction([NotNull] IConfiguration functionSection)
		{
			if (functionSection == null)
			{
				return null;
			}

			var functionName = functionSection.GetValue<string>("function");
			if (string.IsNullOrEmpty(functionName))
			{
				throw new Exception("Function name is missing!");
			}
			
			var comment = functionSection.GetValue<string>("comment");
			if (FunctionFactory.TryCreateFunction(functionName, out var function))
			{
				if (function.Parameters != null)
				{
					var functionParameterSections = functionSection.GetSection("parameters");
					foreach (var functionParameter in function.Parameters)
					{
						var value = functionParameterSections.GetValue(functionParameter.Type, functionParameter.Name);
						if (value != null)
						{
							functionParameter.SetValue(value);	
						}
					}
				}

				function.Comment = comment;
			}

			return function as IInteractionFunction;
		}
		
		#endregion
	}
}