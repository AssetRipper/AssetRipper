using AssetRipper.Core.Logging;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace AssetRipper.GUI
{
	public sealed partial class LocalizationManager : BaseViewModel
	{
		private const string LocalizationFilePrefix = "AssetRipper.GUI.";

		// ReSharper disable once MemberInitializerValueIgnored
		private Dictionary<string, string> CurrentLocale; //To suppress warning as it's initialized indirectly in constructor
		private readonly Dictionary<string, string> FallbackLocale;
		private string? CurrentLang;
		public SupportedLanguage[] SupportedLanguages { get; private set; }

		public event Action OnLanguageChanged = () => { };

		public LocalizationManager()
		{
			LoadLanguage("en_US");
			FallbackLocale = CurrentLocale;

			string[] supportedLanguageCodes = Assembly.GetExecutingAssembly()
				.GetManifestResourceNames()
				.Where(l => l.StartsWith(LocalizationFilePrefix))
				.Select(l => l[LocalizationFilePrefix.Length..^5])
				.ToArray();

			SupportedLanguages = new SupportedLanguage[supportedLanguageCodes.Length];
			for (int i = 0; i < supportedLanguageCodes.Length; i++)
			{
				SupportedLanguages[i] = new SupportedLanguage(this, LanguageNameDictionary[supportedLanguageCodes[i]], supportedLanguageCodes[i]);
			}
		}

		[MemberNotNull(nameof(CurrentLocale), nameof(CurrentLang))]
		public void LoadLanguage(string code)
		{
			CurrentLang = code;
			Logger.Info(LogCategory.System, $"Loading locale {code}.json");
			using System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(LocalizationFilePrefix + code + ".json") ?? throw new Exception($"Could not load language file {code}.json");

			CurrentLocale = JsonSerializer.Deserialize<Dictionary<string, string>>(stream) ?? throw new Exception($"Could not parse language file {code}.json");

			OnPropertyChanged("Item");
			OnPropertyChanged("Item[]");
			OnLanguageChanged();
		}

		public string this[string key]
		{
			get
			{
				if (CurrentLocale.TryGetValue(key, out string? ret) && !string.IsNullOrEmpty(ret))
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
			public LocalizationManager Manager { get; }

			public bool IsActive
			{
				get => Manager.CurrentLang == LanguageCode;
			}

			public SupportedLanguage(LocalizationManager manager, string displayName, string languageCode)
			{
				DisplayName = displayName;
				LanguageCode = languageCode;

				Manager = manager;
				Manager.OnLanguageChanged += () => OnPropertyChanged(nameof(IsActive));

				Logger.Verbose(LogCategory.System, $"Language {displayName} isActive {IsActive}");
			}

			public void Apply()
			{
				Manager.LoadLanguage(LanguageCode);
			}

			public override string ToString() => DisplayName;
		}
	}
}
