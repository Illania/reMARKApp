using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    [Table("Folder")]
    public class Folder : ICopiable<Folder>
    {
        [Column("Id")]
        [PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("Guid")]
        public Guid Guid { get; set; }

        [Column("ParentFolderId")]
        [Indexed]
        public int ParentFolderId { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Module")]
        public ModuleType Module { get; set; }

        [Column("Type")]
        public FolderType Type { get; set; }

        [Column("InternalType")]
        public FolderInternalType InternalType { get; set; }

        [Column("HasSubFolders")]
        public bool HasSubFolders { get; set; }

        [Column("Path")]
        public string Path { get; set; }

        [Ignore]
        public bool Local { get { return DocumentsLocalRootFolder.SubFolders.Any(f => f.Id == Id); } }

        List<Folder> subFolders;

        [Ignore]
        public List<Folder> SubFolders
        {
            get
            {
                if (subFolders == null)
                    subFolders = new List<Folder>();
                return subFolders;
            }
            set => subFolders = value;
        }

        [Column("Subscribed")]
        public bool Subscribed { get; set; }

        [Column("Position")]
        public int Position { get; set; }

        [Ignore]
        public OptionalParameters OptionalParameters { get; set; }

        #region Serialization

        [Column("OptionalParametersString")]
        public string OptionalParametersString { get => Serializer.Serialize(OptionalParameters); set => OptionalParameters = Serializer.Deserialize<OptionalParameters>(value); }

        #endregion

        #region ICopiable

        public Folder ShallowCopy()
        {
            return new Folder
            {
                Id = Id,
                Guid = Guid,
                ParentFolderId = ParentFolderId,
                Name = Name,
                Module = Module,
                Type = Type,
                InternalType = InternalType,
                HasSubFolders = HasSubFolders,
                Subscribed = Subscribed,
                Position = Position,
                Path = Path,
                OptionalParameters = OptionalParameters?.ShallowCopy()
            };
        }

        public Folder DeepCopy()
        {
            var copy = ShallowCopy();
            copy.SubFolders.AddRange(SubFolders.Select(f => f.DeepCopy()));
            return copy;
        }

        #endregion

        #region Root folders

        [Ignore]
        public bool Root => new[]
        {
            DocumentsRootFolder,
            ContactsRootFolder,
            ShortcodesRootFolder,
            CalendarRootFolder
        }.Select(f => f.Id).Contains(Id);

        static readonly Folder DocumentsRootFolder = new Folder
        {
            Id = -100,
            Guid = new Guid("{00000000-0000-0000-0000-000000000100}"),
            Name = "DOCUMENTS_ROOT",
            Module = ModuleType.Documents,
            Type = FolderType.None,
            HasSubFolders = true
        };

        static readonly Folder DocumentsFavoriteRootFolder = new Folder
        {
            Id = -110,
            Guid = new Guid("{00000000-0000-0000-0000-000000000110}"),
            Name = "DOCUMENTS_FAVORITE_ROOT",
            Module = ModuleType.Documents,
            Type = FolderType.None,
            HasSubFolders = true
        };

        static readonly Folder DocumentsOutgoingFolder = new Folder
        {
            Id = -121,
            Guid = new Guid("{00000000-0000-0000-0000-000000000121}"),
            Name = "Outgoing",
            Module = ModuleType.Documents,
            Type = FolderType.None,
            HasSubFolders = false
        };

        static readonly Folder DocumentsLocalRootFolder = new Folder
        {
            Id = -120,
            Guid = new Guid("{00000000-0000-0000-0000-000000000120}"),
            Name = "DOCUMENTS_LOCAL_ROOT",
            Module = ModuleType.Documents,
            Type = FolderType.None,
            HasSubFolders = true,
            SubFolders =
            {
                DocumentsOutgoingFolder
            }
        };

        static readonly Folder ContactsRootFolder = new Folder
        {
            Id = -200,
            Guid = new Guid("{00000000-0000-0000-0000-000000000200}"),
            Name = "CONTACTS_ROOT",
            Module = ModuleType.Contacts,
            Type = FolderType.None,
            HasSubFolders = true
        };

        static readonly Folder ContactsFavoriteRootFolder = new Folder
        {
            Id = -210,
            Guid = new Guid("{00000000-0000-0000-0000-000000000210}"),
            Name = "CONTACTS_FAVORITE_ROOT",
            Module = ModuleType.Contacts,
            Type = FolderType.None,
            HasSubFolders = true
        };

        static readonly Folder ShortcodesRootFolder = new Folder
        {
            Id = -300,
            Guid = new Guid("{00000000-0000-0000-0000-000000000300}"),
            Name = "SHORTCODES_ROOT",
            Module = ModuleType.Shortcodes,
            Type = FolderType.None,
            HasSubFolders = true
        };

        static readonly Folder ShortcodesFavoriteRootFolder = new Folder
        {
            Id = -310,
            Guid = new Guid("{00000000-0000-0000-0000-000000000310}"),
            Name = "SHORTCODES_FAVORITE_ROOT",
            Module = ModuleType.Shortcodes,
            Type = FolderType.None,
            HasSubFolders = true
        };

        static readonly Folder CalendarRootFolder = new Folder
        {
            Id = -400,
            Guid = new Guid("{00000000-0000-0000-0000-000000000400}"),
            Name = "CALENDAR_ROOT",
            Module = ModuleType.Calendar,
            Type = FolderType.None,
            HasSubFolders = true
        };

        static readonly Folder CalendarFavoriteRootFolder = new Folder
        {
            Id = -410,
            Guid = new Guid("{00000000-0000-0000-0000-000000000410}"),
            Name = "CALENDAR_FAVORITE_ROOT",
            Module = ModuleType.Calendar,
            Type = FolderType.None,
            HasSubFolders = true
        };

        public static Folder RootForModule(ModuleType module)
        {
            switch (module)
            {
                case ModuleType.Documents:
                    return DocumentsRootFolder;
                case ModuleType.Contacts:
                    return ContactsRootFolder;
                case ModuleType.Shortcodes:
                    return ShortcodesRootFolder;
                case ModuleType.Calendar:
                    return CalendarRootFolder;
                default:
                    throw new ArgumentException(nameof(module));
            }
        }

        public static Folder FavoritesRootForModule(ModuleType module)
        {
            switch (module)
            {
                case ModuleType.Documents:
                    return DocumentsFavoriteRootFolder;
                case ModuleType.Contacts:
                    return ContactsFavoriteRootFolder;
                case ModuleType.Shortcodes:
                    return ShortcodesFavoriteRootFolder;
                case ModuleType.Calendar:
                    return CalendarFavoriteRootFolder;
                default:
                    throw new ArgumentException(nameof(module));
            }
        }

        public static Folder LocalRootForModule(ModuleType module)
        {
            switch (module)
            {
                case ModuleType.Documents:
                    return DocumentsLocalRootFolder;
                default:
                    throw new ArgumentException("Input module not valid");
            }
        }

        public bool IsOutgoing => Id == DocumentsOutgoingFolder.Id;

        #endregion

        public override string ToString()
        {
            return $"[Folder: Id={Id}, ParentFolderId={ParentFolderId}, Name={Name}, Module={Module}]";
        }

    }

    public class FolderComparer : IEqualityComparer<Folder> {

        public bool Equals(Folder x, Folder y)
        {
            return x != null && y != null && x.Id.Equals(y.Id);
        }

        public int GetHashCode(Folder obj)
        {
            return obj.Id;
        }

    }
}