//
// Project: Mark5.Mobile.Common
// File: Folder.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;
using System.Linq;

namespace Mark5.Mobile.Common.Model
{

    [Table("Folder")]
    public class Folder : ICopiable<Folder>
    {

        [Column("Id"), PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("Guid")]
        public Guid Guid { get; set; }

        [Column("ParentFolderId"), Indexed]
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

        List<Folder> subFolders;

        [Ignore]
        public List<Folder> SubFolders
        {
            get
            {
                if (subFolders == null)
                {
                    subFolders = new List<Folder>();
                }

                return subFolders;
            }
            set
            {
                subFolders = value;
            }
        }

        [Column("Subscribed")]
        public bool Subscribed { get; set; }

        [Column("Position")]
        public int Position { get; set; }

        [Ignore]
        public OptionalParameters OptionalParameters { get; set; }

        #region Serialization

        [Column("OptionalParametersBytes")]
        public byte[] OptionalParametersBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(OptionalParameters);
            }
            set
            {
                OptionalParameters = SerializationUtils.DeserializeFromByteArray<OptionalParameters>(value);
            }
        }

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
                Position = Position,
            };
        }

        public Folder DeepCopy()
        {
            var copy = ShallowCopy();
            copy.OptionalParameters = OptionalParameters?.DeepCopy();
            copy.SubFolders.AddRange(SubFolders.Select(f => f.DeepCopy()));
            return copy;
        }

        #endregion

        public override string ToString()
        {
            return $"[Folder: Id={Id}, ParentFolderId={ParentFolderId}, Name={Name}, Module={Module}]";
        }
    }
}

