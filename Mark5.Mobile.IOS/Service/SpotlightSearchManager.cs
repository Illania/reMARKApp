using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreSpotlight;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using MobileCoreServices;

namespace Mark5.Mobile.IOS.Service
{
	public class SpotlightSearchManager : ISpotlightSearchManager
	{
		/// <summary>
        /// Adds or Updates ContactPreview items in Spotlight Search Index
        /// </summary>
        /// <param name="items"></param>
		public void AddOrUpdateContactsToIndex(List<ContactPreview> items)
		{
			foreach (var item in items)
			{
				AddOrUpdateContactToIndex(item);
			}
		}

		/// <summary>
        /// Removes all ContactPreview items in Spotlight Search Index that are not in "items" anymore
        /// </summary>
        /// <param name="items">Current ContactPreview items in database</param>
		public async void RemoveDeletedContactsFromIndex(List<ContactPreview> items)
		{
			async void DeleteItems(CSSearchableItem[] foundItems)
			{

				foreach (var foundItem in foundItems)
				{
					try
					{
						await Managers.ContactsManager.GetContactAsync(-1, Convert.ToInt32(foundItem.UniqueIdentifier));
					}
					catch (Exception)
                    {
						DeleteContactFromIndex(foundItem);
					}
				}
			}

			foreach (var item in items)
			{
				void HandleError(NSError error)
				{
					if (error != null)
						CommonConfig.Logger.Error($"Failed deleting contact item (Id={item.Id}) from  Spotlight search index. Error: {error}");
				}

				var queryString = @$"identifier == ""{item.Id}""";

				var searchQuery = new CSSearchQuery(queryString, (string[])null)
				{
					FoundItemsHandler = DeleteItems,
					CompletionHandler = HandleError
				};

				searchQuery?.Start();
			}
			
        }

		/// <summary>
		/// Deletes ComtactPreview items from Spotlight Search Index
		/// </summary>
		/// <param name="items">ContactPreview items to delete from index</param>
		public void DeleteContactsFromIndex(List<ContactPreview> items)
		{
			foreach(var item in items)
            {
				var queryString = @$"identifier == ""{item.Id}""";

				var searchQuery = new CSSearchQuery(queryString, (string[])null)
				{
					FoundItemsHandler = DeleteItems,
					CompletionHandler = HandleError
				};

				void DeleteItems(CSSearchableItem[] foundItems)
				{
					foreach (var foundItem in foundItems)
						DeleteContactFromIndex(foundItem);
				}

				void HandleError(NSError error)
				{
					if (error != null)
						CommonConfig.Logger.Error($"Failed deleting contact item (Id={item.Id}) from  Spotlight search index. Error: {error}");
				}

				searchQuery?.Start();
			}
		}

		void AddOrUpdateContactToIndex(ContactPreview item)
		{
			var queryString = @$"identifier == ""{item.Id}""";
			var found = false;
			var searchQuery = new CSSearchQuery(queryString, (string[])null)
			{
				FoundItemsHandler = AddOrUpdateItem,
				CompletionHandler = HandleError
			};

			void AddOrUpdateItem(CSSearchableItem[] foundItems)
			{
				found = foundItems.Count() > 0;
				foreach (var foundItem in foundItems)
				{
					DeleteContactFromIndex(foundItem);
					AddContactToIndex(item);
				}
			}

			void HandleError(NSError error)
			{
				if (found == false)
					AddContactToIndex(item);

				if (error != null)
					CommonConfig.Logger.Error($"Failed indexing contact item (Id={item.Id}) for  Spotlight search. Error: {error}");
			}

			searchQuery?.Start();
		}

		void AddContactToIndex(ContactPreview item)
		{
			// Create attributes to describe item
			var attributes = new CSSearchableItemAttributeSet(UTType.Text)
			{
				Title = item.Name,
				ContentDescription = item.Description,
				Identifier = item.Id.ToString(),
            };                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           

            // Create item
            var searchableItem = new CSSearchableItem($"{item.Id}", "com.nordic-it.mark5.mobile.ios", attributes);
			// Index item
			CSSearchableIndex.DefaultSearchableIndex.Index(new CSSearchableItem[] { searchableItem }, error =>
			{
				if (error != null)
				{
					CommonConfig.Logger.Error($"Failed indexing contact item (Id={item.Id}) for  Spotlight search. Error: {error}");
				}
				else
				{
					CommonConfig.Logger.Info($"Successfully indexed contact item (Id={item.Id}) for  Spotlight search.");
				}
			});
		}

		void DeleteContactFromIndex(CSSearchableItem item)
		{
			CSSearchableIndex.DefaultSearchableIndex.Delete(new string[] { item.UniqueIdentifier }, error =>
			{
				if (error != null)
				{
					CommonConfig.Logger.Error($"Failed deleting contact item (Id={item.UniqueIdentifier}) from  Spotlight search index. Error: {error}");
				}
				else
				{
					Debug.WriteLine($"Successfully deleted contact item (Id={item.UniqueIdentifier}) from  Spotlight search index.");
				}
			});
		}
	}
}