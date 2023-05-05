﻿namespace ContactPro_Crucible.Services.Interfaces
{
	public interface IAddressBookService
	{
		public Task AddCategoriesToContactAsync(IEnumerable<int> categoryIds, int contactId);
		public Task RemoveCategoriesFromContactAsync(int contactId);
	}
}
