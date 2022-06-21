using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Service
{ 
	public interface ISpotlightSearchManager
	{
		void AddOrUpdateContactsToIndex(List<ContactPreview> items);
		void RemoveDeletedContactsFromIndex(List<ContactPreview> items);
		void DeleteContactsFromIndex(List<ContactPreview> items);
	}
}
