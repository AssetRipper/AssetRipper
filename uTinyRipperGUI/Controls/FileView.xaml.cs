using System;
using System.Collections.Generic;
using System.Windows.Controls;
using uTinyRipper;
using uTinyRipper.SerializedFiles;

namespace uTinyRipperGUI.Controls
{
	public partial class FileView : UserControl
	{
		private class FileEntry
		{
			public FileEntry(string name)
			{
				Name = name ?? throw new ArgumentNullException(nameof(name));
				Children = new FileEntry[0];
			}

			public FileEntry(string name, IReadOnlyList<FileEntry> children) :
				this(name)
			{
				Children = children ?? throw new ArgumentNullException(nameof(children));
			}

			public override string ToString()
			{
				return Name == null ? base.ToString() : $"N:{Name} CC:{Children.Count}";
			}

			public string Name { get; set; }
			public IReadOnlyList<FileEntry> Children { get; set; }
		}

		public FileView()
		{
			InitializeComponent();

			Treeview.ItemsSource = m_entries;
		}

		public void AddItem(GameCollection collection)
		{
			Clear();
			AddChildrenItems(m_entries, collection);
		}

		private void AddItem(IList<FileEntry> entries, SerializedFile file)
		{
			int index = 0;
			FileEntry[] children = new FileEntry[file.Metadata.Object.Length];
			foreach (ObjectInfo entry in file.Metadata.Object)
			{
				children[index++] = new FileEntry($"{entry.FileID}. {entry.ClassID}");
			}
			entries.Add(new FileEntry(file.Name, children));
		}

		private void AddItem(IList<FileEntry> entries, FileList list)
		{
			List<FileEntry> children = new List<FileEntry>(list.SerializedFiles.Count + list.FileLists.Count + list.ResourceFiles.Count);
			AddChildrenItems(children, list);
			entries.Add(new FileEntry(list.Name, children));
		}

		private void AddItem(IList<FileEntry> entries, ResourceFile file)
		{
			entries.Add(new FileEntry(file.Name));
		}

		private void AddChildrenItems(IList<FileEntry> children, FileList list)
		{
			foreach (SerializedFile file in list.SerializedFiles)
			{
				AddItem(children, file);
			}
			foreach (FileList nestedList in list.FileLists)
			{
				AddItem(children, nestedList);
			}
			foreach (ResourceFile file in list.ResourceFiles)
			{
				AddItem(children, file);
			}
		}

		public void Refresh()
		{
			Treeview.Items.Refresh();
		}

		public void Clear()
		{
			m_entries.Clear();
			Refresh();
		}

		private readonly IList<FileEntry> m_entries = new List<FileEntry>();
	}
}
