namespace Mark5.Mobile.Common.Storage.AppFileStorage.Enum
{
	/// <summary>
	/// Specifies what should happen when trying to create/rename a file or folder to a name that already exists.
	/// </summary>
	public enum NameCollisionOption
	{
		/// <summary>
		/// Automatically generate a unique name by appending a number to the name of
		/// the file or folder.
		/// </summary>
		GenerateUniqueName,
		/// <summary>
		/// Replace the existing file or folder. Your app must have permission to access
		/// the location that contains the existing file or folder.
		/// </summary>
		ReplaceExisting,
		/// <summary>
		/// Return an error if another file or folder exists with the same name and abort
		/// the operation.
		/// </summary>
		FailIfExists
	}

}
