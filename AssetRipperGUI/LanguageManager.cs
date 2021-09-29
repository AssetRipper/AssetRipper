using AssetRipper.Core.Extensions;
using AssetRipper.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace AssetRipper.GUI
{
	public class LanguageManager : BaseViewModel
	{
		private const string LocalizationFilePrefix = "AssetRipper.GUI.Localizations.";

		// ReSharper disable once MemberInitializerValueIgnored
		private Dictionary<string, string> CurrentLocale = null!; //To suppress warning as it's initialized indirectly in constructor
		private Dictionary<string, string> FallbackLocale;
		private string? _lastLoadedLang;
		public SupportedLanguage[] SupportedLanguages { get; }

		public event Action OnLanguageChanged = () => { };

		public LanguageManager()
		{
			LoadLanguage("en_US");
			FallbackLocale = CurrentLocale;

			var supportedLanguageCodes = Assembly.GetExecutingAssembly()
				.GetManifestResourceNames()
				.Where(l => l.StartsWith(LocalizationFilePrefix))
				.Select(l => l[LocalizationFilePrefix.Length..^5])
				.ToArray();

			var supportedLanguageNames = supportedLanguageCodes.Select(code => new CultureInfo(code)).Select(culture => culture.DisplayName).ToArray();

			List<SupportedLanguage> languages = new();
			for (int i = 0; i < supportedLanguageNames.Length; i++)
			{
				languages.Add(new(supportedLanguageNames[i], supportedLanguageCodes[i]));
			}

			SupportedLanguages = languages.ToArray();
		}

		[SuppressMessage("ReSharper", "NotResolvedInText")]
		public void LoadLanguage(string code)
		{
			_lastLoadedLang = code;
			Logger.Info(LogCategory.System, $"Loading locale {code}.json");
			using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(LocalizationFilePrefix + code + ".json") ?? throw new Exception($"Could not load language file {code}.json");
			
			CurrentLocale = JsonSerializer.Deserialize<Dictionary<string, string>>(stream) ?? throw new Exception($"Could not parse language file {code}.json");
			
			OnPropertyChanged("Item");
			OnPropertyChanged("Item[]");
			OnLanguageChanged();
		}

		public string this[string key]
		{
			get
			{
				if (CurrentLocale.TryGetValue(key, out var ret) && !string.IsNullOrEmpty(ret))
				{
					return ret;
				}

				if (FallbackLocale.TryGetValue(key, out ret))
				{
					Logger.Warning(LogCategory.System, $"Locale {_lastLoadedLang} is missing a definition for {key}. Using fallback language (en_US)");
					return ret;
				}

				Logger.Error(LogCategory.System, $"Locale {_lastLoadedLang} is missing a definition for {key}, and it also could not be found in the fallback language (en_US)");
				return $"__{key}__?";
			}
		}

		public class SupportedLanguage
		{
			public string DisplayName { get; }
			public string LanguageCode { get; }

			public SupportedLanguage(string displayName, string languageCode)
			{
				DisplayName = displayName;
				LanguageCode = languageCode;
			}

			public void Apply()
			{
				MainWindow.Instance.LanguageManager.LoadLanguage(LanguageCode);
			}

			public override string ToString() => DisplayName;
		}
	}
}