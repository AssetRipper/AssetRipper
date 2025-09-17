#nullable disable

using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.AssemblyDumper.InjectedTypes;

internal sealed class FetchDependenciesExampleObject : UnityObjectBase
{
	private PPtr<IUnityObjectBase> field0;
	private PPtr<IUnityObjectBase> field1;
	private AssetList<PPtr<IUnityObjectBase>> field2;
	private AssetDictionary<PPtr<IUnityObjectBase>, PPtr<IUnityObjectBase>> field3;
	private AssetList<AssetList<PPtr<IUnityObjectBase>>> field4;
	private AssetPair<PPtr<IUnityObjectBase>, PPtr<IUnityObjectBase>> field5;

	public FetchDependenciesExampleObject(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public new IEnumerable<(string, PPtr)> FetchDependencies()
	{
		return new FetchDependenciesEnumerable(this);
	}

	internal sealed class FetchDependenciesEnumerable : FetchDependenciesEnumerableBase<FetchDependenciesExampleObject>
	{
		private int _index;
		private int _field2_0_i;
		private int _field3_0_i;
		private bool _field3_0_value;
		private int _field4_0_i;
		private int _field4_1_i;
		private bool _field5_value;

		public FetchDependenciesEnumerable(FetchDependenciesExampleObject @this) : base(@this)
		{
		}

		private protected override FetchDependenciesEnumerableBase<FetchDependenciesExampleObject> CreateNew()
		{
			return new FetchDependenciesEnumerable(_this);
		}

		public override bool MoveNext()
		{
			switch (_index)
			{
				case 0:
					_current = (nameof(_this.field0), _this.field0);
					_index++;
					break;

				case 1:
					_current = (nameof(_this.field1), _this.field1);
					_index++;
					break;

				case 2:
					if (_field2_0_i < _this.field2.Count)
					{
						_current = ("field2[]", _this.field2[_field2_0_i]);
						_field2_0_i++;
					}
					else
					{
						_index++;
						goto case 3;
					}
					break;

				case 3:
					if (_field3_0_i < _this.field3.Count)
					{
						if (!_field3_0_value)
						{
							_current = ("field3[].Key", _this.field3.GetPair(_field3_0_i).Key);
							_field3_0_value = true;
						}
						else
						{
							_current = ("field3[].Value", _this.field3.GetPair(_field3_0_i).Value);
							_field3_0_value = false;
							_field3_0_i++;
						}
					}
					else
					{
						_index++;
						goto case 4;
					}
					break;

				case 4:
					if (_field4_0_i < _this.field4.Count)
					{
						if (_field4_1_i < _this.field4[_field4_0_i].Count)
						{
							_current = ("field4[][]", _this.field4[_field4_0_i][_field4_1_i]);
							_field4_1_i++;
						}
						else
						{
							_field4_0_i++;
							_field4_1_i = 0;
							goto case 4;
						}
					}
					else
					{
						_index++;
						goto case 5;
					}
					break;

				case 5:
					if (!_field5_value)
					{
						_current = ("field5.Key", _this.field5.Key);
						_field5_value = true;
					}
					else
					{
						_current = ("field5.Value", _this.field5.Value);
						_field5_value = false;
						_index++;
					}
					break;

				default:
					return false;
			}
			return true;
		}
	}
}

#nullable enable
