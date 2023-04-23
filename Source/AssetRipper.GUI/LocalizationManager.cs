using AssetRipper.GUI.Localizations;
using AssetRipper.Import.Logging;

namespace AssetRipper.GUI
{
	public sealed class LocalizationManager : BaseViewModel
	{
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

			SupportedLanguages = LocalizationLoader.LanguageNameDictionary.Select(pair => new SupportedLanguage(this, pair.Value, pair.Key)).ToArray();
		}

		[MemberNotNull(nameof(CurrentLocale), nameof(CurrentLang))]
		public void LoadLanguage(string code)
		{
			CurrentLang = code;
			Logger.Info(LogCategory.System, $"Loading locale {code}.json");
			CurrentLocale = LocalizationLoader.LoadLanguage(code);
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
