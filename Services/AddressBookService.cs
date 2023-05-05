using ContactPro_Crucible.Data;
using ContactPro_Crucible.Models;
using ContactPro_Crucible.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContactPro_Crucible.Services
{
	public class AddressBookService : IAddressBookService
	{
		private readonly ApplicationDbContext _context;  //connection to db
		public AddressBookService(ApplicationDbContext context) //constructor
		{
			_context = context; //injection of db
		}

		public async Task AddCategoriesToContactAsync(IEnumerable<int> categoryIds, int contactId)
		{
			try
			{
				Contact? contact = await _context.Contacts
												 .Include(c => c.Categories)
												 .FirstOrDefaultAsync(c => c.Id == contactId);

				foreach (int categoryId in categoryIds) //"see if they have created or selected any categories"
				{
					Category? category = await _context.Categories.FindAsync(categoryId); //instantiate the category based off id, query category based off id

					if (contact != null && category != null) //if contact is not null and category is not null, add the category
					{
						contact.Categories.Add(category); //navigation property for the relationship; add the object(category) to collection of categories
					}
				}
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}

		public async Task RemoveCategoriesFromContactAsync(int contactId)
		{
			try
			{
				Contact? contact = await _context.Contacts
												 .Include(c => c.Categories)
												 .FirstOrDefaultAsync(c => c.Id == contactId);

				contact!.Categories.Clear();
				_context.Update(contact);
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
