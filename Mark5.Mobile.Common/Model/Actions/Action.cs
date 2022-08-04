using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model.Actions
{
    public abstract class Action
    {
        public ActionType Type { get; }

        public ObjectType ObjectType { get; }

        [Column("Guid")]
        [PrimaryKey]
        public Guid Guid { get; set; }

        [Column("Date")]
        public DateTime CreatedDate { get; set; }

        protected Action(ActionType type, ObjectType objectType)
        {
            Type = type;
            ObjectType = objectType;
            Guid = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }
    }

    [Table("SetReadStatusAction")]
    public class SetReadStatusAction : Action
    {
        [Column("ReadStatus")]
        public bool ReadStatus { get; set; }

        [Ignore]
        public List<int> DocumentIds { get; private set; }

        [Column("DocumentIdsString")]
        public string DocumentIdsString { get => Serializer.Serialize(DocumentIds); set => DocumentIds = Serializer.Deserialize<List<int>>(value); }

        public SetReadStatusAction() : base(ActionType.SetReadStatus, ObjectType.Document)
        { }

        private SetReadStatusAction(bool readStatus, List<int> documentIds)
            : base(ActionType.SetReadStatus, ObjectType.Document)
        {
            ReadStatus = readStatus;
            DocumentIds = documentIds;
        }

        public static SetReadStatusAction Create(bool readStatus, params int[] documentIds)
        {
            return new SetReadStatusAction(readStatus, documentIds.ToList());
        }
    }

    [Table("SetCategoriesAction")]
    public class SetCategoriesAction : Action
    {

        [Ignore]
        public List<Category> Categories { get; private set; }

        [Column("ObjectId")]
        public int ObjectId { get; set; }

        [Column("CategoryString")]
        public string CategoriesString { get => Serializer.Serialize(Categories); set => Categories = Serializer.Deserialize<List<Category>>(value); }

        public SetCategoriesAction() : base(ActionType.SetCategories, ObjectType.Document)
        { }

        private SetCategoriesAction(List<Category> categories, int objectId, ObjectType objectType)
            : base(ActionType.SetCategories, objectType)
        {
            Categories = categories;
            ObjectId = objectId;
        }

        public static SetCategoriesAction Create(List<Category> categories, int objectId, ObjectType objectType)
        {
            return new SetCategoriesAction(categories, objectId, objectType);
        }
    }


    [Table("CopyToFolderAction")]
    public class CopyToFolderAction : Action
    {
        [Column("FolderId")]
        public int FolderId { get; set; }

        [Ignore]
        public List<int> DocumentIds { get; private set; }

        [Column("DocumentIdsString")]
        public string DocumentIdsString { get => Serializer.Serialize(DocumentIds); set => DocumentIds = Serializer.Deserialize<List<int>>(value); }

        public CopyToFolderAction() : base(ActionType.CopyToFolder, ObjectType.Document)
        { }

        private CopyToFolderAction(List<int> documentIds, int folderId, ObjectType objectType)
            : base(ActionType.CopyToFolder, objectType)
        {
            FolderId = folderId;
            DocumentIds = documentIds;
        }

        public static CopyToFolderAction Create(int folderId, ObjectType objectType, params int[] documentIds)
        {
            return new CopyToFolderAction(documentIds.ToList(), folderId, objectType);
        }
    }


    [Table("MoveToFolderAction")]
    public class MoveToFolderAction : Action
    {
        [Column("FromFolderId")]
        public int FromFolderId { get; set; }

        [Column("ToFolderId")]
        public int ToFolderId { get; set; }

        [Ignore]
        public List<int> DocumentIds { get; private set; }

        [Column("DocumentIdsString")]
        public string DocumentIdsString { get => Serializer.Serialize(DocumentIds); set => DocumentIds = Serializer.Deserialize<List<int>>(value); }

        public MoveToFolderAction() : base(ActionType.MoveToFolder, ObjectType.Document)
        { }

        private MoveToFolderAction(List<int> documentIds, int fromFolderId, int toFolderId, ObjectType objectType)
            : base(ActionType.CopyToFolder, objectType)
        {
            FromFolderId = fromFolderId;
            ToFolderId = toFolderId;
            DocumentIds = documentIds;
        }

        public static MoveToFolderAction Create(int fromFolderId, int toFolderId, ObjectType objectType, params int[] documentIds)
        {
            return new MoveToFolderAction(documentIds.ToList(), fromFolderId, toFolderId, objectType);
        }
    }


    [Table("CopyToWorktrayAction")]
    public class CopyToWorktrayAction : Action
    {

        [Ignore]
        public List<int> DocumentIds { get; private set; }

        [Column("DocumentIdsString")]
        public string DocumentIdsString { get => Serializer.Serialize(DocumentIds); set => DocumentIds = Serializer.Deserialize<List<int>>(value); }

        public CopyToWorktrayAction() : base(ActionType.CopyToWorktray, ObjectType.Document)
        { }

        private CopyToWorktrayAction(List<int> documentIds, ObjectType objectType)
            : base(ActionType.CopyToWorktray, objectType)
        {
            DocumentIds = documentIds;
        }

        public static CopyToWorktrayAction Create(ObjectType objectType, params int[] documentIds)
        {
            return new CopyToWorktrayAction(documentIds.ToList(), objectType);
        }
    }


    [Table("RemoveFromFolderAction")]
    public class RemoveFromFolderAction : Action
    {

        [Column("FolderId")]
        public int FolderId { get; set; }

        [Ignore]
        public List<int> DocumentIds { get; private set; }

        [Column("DocumentIdsString")]
        public string DocumentIdsString { get => Serializer.Serialize(DocumentIds); set => DocumentIds = Serializer.Deserialize<List<int>>(value); }

        public RemoveFromFolderAction() : base(ActionType.RemoveFromFolder, ObjectType.Document)
        { }

        private RemoveFromFolderAction(List<int> documentIds, int folderId, ObjectType objectType)
            : base(ActionType.RemoveFromFolder, objectType)
        {
            DocumentIds = documentIds;
            FolderId = folderId;
        }

        public static RemoveFromFolderAction Create(int folderId, ObjectType objectType, params int[] documentIds)
        {
            return new RemoveFromFolderAction(documentIds.ToList(), folderId, objectType);
        }
    }

    [Table("DeleteAction")]
    public class DeleteAction : Action
    {

        [Ignore]
        public List<int> DocumentIds { get; private set; }

        [Column("DocumentIdsString")]
        public string DocumentIdsString { get => Serializer.Serialize(DocumentIds); set => DocumentIds = Serializer.Deserialize<List<int>>(value); }

        public DeleteAction() : base(ActionType.Delete, ObjectType.Document)
        { }

        private DeleteAction(List<int> documentIds, ObjectType objectType)
            : base(ActionType.Delete, objectType)
        {
            DocumentIds = documentIds;
        }

        public static DeleteAction Create(ObjectType objectType, params int[] documentIds)
        {
            return new DeleteAction(documentIds.ToList(), objectType);
        }
    }



    public enum ActionType
    {
        SetReadStatus,
        SetCategories,
        CopyToFolder,
        CopyToWorktray,
        MoveToFolder,
        RemoveFromFolder,
        Delete  
    }
}
