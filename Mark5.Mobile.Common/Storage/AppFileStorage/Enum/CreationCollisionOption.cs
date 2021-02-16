namespace Mark5.Mobile.Common.Storage.AppFileStorage.Enum
{
	public enum CreationCollisionOption
    {
    /// <summary>
	/// Creates a new file with a unique name of the form "name (2).txt"
	/// </summary>
	GenerateUniqueName,
	/// <summary>
	/// Replaces any existing file with a new (empty) one
	/// </summary>
	ReplaceExisting,
	/// <summary>
	/// Throws an exception if the file exists
	/// </summary>
	FailIfExists,
	/// <summary>
	/// Opens the existing file, if any
	/// </summary>
	OpenIfExists

    }
}