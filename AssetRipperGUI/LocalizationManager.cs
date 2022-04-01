using AssetRipper.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AssetRipper.GUI
{
	public class LocalizationManager : BaseViewModel
	{
		private const string LocalizationFilePrefix = "AssetRipper.GUI.";
		private static readonly Regex SortOrderRegex = new("\\(Sort Order=([A-Z]+)\\)", RegexOptions.Compiled);

		// ReSharper disable once MemberInitializerValueIgnored
		private Dictionary<string, string> CurrentLocale = null!; //To suppress warning as it's initialized indirectly in constructor
		private Dictionary<string, string> FallbackLocale;
		private string? CurrentLang;
		public SupportedLanguage[] SupportedLanguages { get; private set; }

		public event Action OnLanguageChanged = () => { };

		public LocalizationManager()
		{

		}

		public void Init()
		{
			LoadLanguage("en_US");
			FallbackLocale = CurrentLocale;

			var supportedLanguageCodes = Assembly.GetExecutingAssembly()
				.GetManifestResourceNames()
				.Where(l => l.StartsWith(LocalizationFilePrefix))
				.Select(l => l[LocalizationFilePrefix.Length..^5])
				.ToArray();

			var supportedLanguageNames = supportedLanguageCodes.Select(code => new CultureInfo(code.Replace('_', '-'))).Select(ExtractCultureName).ToArray();

			List<SupportedLanguage> languages = new();
			for (int i = 0; i < supportedLanguageNames.Length; i++)
			{
				languages.Add(new(supportedLanguageNames[i], supportedLanguageCodes[i]));
			}

			SupportedLanguages = languages.ToArray();
		}

		private static string ExtractCultureName(CultureInfo culture)
		{
			return SortOrderRegex.Replace(culture.NativeName, match => $"({match.Groups[1].Value})");
		}

		[SuppressMessage("ReSharper", "NotResolvedInText")]
		public void LoadLanguage(string code)
		{
			CurrentLang = code;
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
					Logger.Verbose(LogCategory.System, $"Locale {CurrentLang} is missing a definition for {key}. Using fallback language (en_US)");
					return ret;
				}

				Logger.Warning(LogCategory.System, $"Locale {CurrentLang} is missing a definition for {key}, and it also could not be found in the fallback language (en_US)");
				return $"__{key}__?";
			}
		}

		public class SupportedLanguage : BaseViewModel
		{
			public string DisplayName { get; }
			public string LanguageCode { get; }

			public bool IsActive
			{
				get => MainWindow.Instance.LocalizationManager.CurrentLang == LanguageCode;
			}

			public SupportedLanguage(string displayName, string languageCode)
			{
				DisplayName = displayName;
				LanguageCode = languageCode;

				Logger.Verbose(LogCategory.System, $"Language {displayName} isActive {IsActive}");

				MainWindow.Instance.LocalizationManager.OnLanguageChanged += () => OnPropertyChanged(nameof(IsActive));
			}

			public void Apply()
			{
				MainWindow.Instance.LocalizationManager.LoadLanguage(LanguageCode);
			}

			public override string ToString() => DisplayName;
		}
	}
}
