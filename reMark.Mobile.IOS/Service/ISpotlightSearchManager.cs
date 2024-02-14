using System.Collections.Generic;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.IOS.Service
{ 
	public interface ISpotlightSearchManager
	{
		void AddOrUpdateContactsToIndex(List<ContactPreview> items);
		void RemoveDeletedContactsFromIndex(List<ContactPreview> items);
		void DeleteContactsFromIndex(List<ContactPreview> items);
	}
}
