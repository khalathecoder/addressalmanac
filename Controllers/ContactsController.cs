using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactPro_Crucible.Data;
using ContactPro_Crucible.Models;
using Microsoft.AspNetCore.Authorization;
using ContactPro_Crucible.Enums;
using Microsoft.AspNetCore.Identity;
using ContactPro_Crucible.Services.Interfaces;
using ContactPro_Crucible.Services;
using ContactPro_Crucible.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ContactPro_Crucible.Controllers

{
	[Authorize] //forces you to login when accessing my contacts page
	public class ContactsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<AppUser> _userManager;
		private readonly IImageService _imageService;
		private readonly IAddressBookService _addressBookService;
		private readonly IEmailSender _emailService;

		//constructors - beginnins of dependency of injection
		public ContactsController(ApplicationDbContext context,
								  UserManager<AppUser> userManager,
								  IImageService imageService,
								  IAddressBookService addressBookService,IEmailSender emailService)
		{
			//dependency injection
			_context = context;
			_userManager = userManager;
			_imageService = imageService;
			_addressBookService = addressBookService;
			_emailService = emailService;
		}

		// GET: Contacts
		public async Task<IActionResult> Index(int? categoryId, string? swalMessage = null) // '= null' makes it optional
		{
			ViewData["SwalMessage"] = swalMessage;

			string? appUserId = _userManager.GetUserId(User); //getting user id

			List<Contact>? contacts = new List<Contact>(); //instanstiate a new list
			

			if (categoryId == null)
			{
				//query for default contacts
				contacts = await _context.Contacts.Where(c => c.AppUserId == appUserId) //get users info
											  .Include(c => c.Categories) //will include categories 
											  .OrderBy(c => c.LastName)
											  .ThenBy(c => c.FirstName)
											  .ToListAsync();  //Comms with the db aka LINQ to retrieve contact and grab all items
			}
			else
			{

				//Query for filtered Contacts by CategoryID
				contacts = (await _context.Categories
										 .Where(c => c.AppUserId == appUserId)
										 .Include(c => c.Contacts)
										 .FirstOrDefaultAsync(c => c.Id == categoryId))!.Contacts.ToList();
			}

			//List<int> categoryIds = new List<int>();
			//categoryIds.Add(categoryId?.Value);


			//TODO: produce the list of categories
			ViewData["Categories"] = await GetCategoriesListAsync();

			return View(contacts); //pass contacts to the view
		}


		public async Task<IActionResult> SearchContacts(string? searchString)
		{
			string? appUserId = _userManager.GetUserId(User);

			List<Contact>? contacts = new List<Contact>();

			AppUser? appUser = await _context.Users
											 .Include(c => c.Contacts)
												.ThenInclude(c => c.Categories)
											 .FirstOrDefaultAsync(u=>u.Id == appUserId);

			if (string.IsNullOrEmpty(searchString))
			{
				contacts = appUser?.Contacts
								  .OrderBy(c=>c.LastName)
								  .ThenBy(c=>c.FirstName)
								  .ToList();
			}
			else
			{
				contacts = appUser?.Contacts
								  .Where(c=>c.FullName!.ToLower().Contains(searchString.ToLower()))
								  .OrderBy(c => c.LastName)
								  .ThenBy(c => c.FirstName)
								  .ToList();
			}



			return View(nameof(Index), contacts); //view of Index and "pass it" the list contacts
		}

		// GET: Contacts/Details/5
		[HttpGet]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Contacts == null)
			{
				return NotFound();
			}

			var contact = await _context.Contacts
				.Include(c => c.AppUser)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (contact == null)
			{
				return NotFound();
			}

			return View(contact);
		}


		[HttpGet]
		public async Task<IActionResult> EmailContact(int? id)
		{
			if (id == null) //if id is null, return not found
			{
				return NotFound();
			}

			string? appUserId = _userManager?.GetUserId(User); //getting user id
			Contact? contact = await _context.Contacts
											 .Where(c => c.AppUserId == appUserId) //this ensures security that no one can change id int
											 .FirstOrDefaultAsync(c => c.Id == id); //instantiate  new object based off id
			if (contact == null) //if contact retrieved is null, return not found
			{
				return NotFound();
			}

			EmailData emailData = new EmailData()
			{
				EmailAddress = contact.Email,
				FirstName = contact.FirstName,
				LastName = contact.LastName
			};

			EmailContactViewModel viewModel = new EmailContactViewModel()
			{
				Contact = contact,
				EmailData = emailData
			};

			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EmailContact(EmailContactViewModel viewModel)
		{
			if(ModelState.IsValid) //checks to make sure required items are valid
			{
				string? swalMessage = string.Empty;

				try
				{
					//create intermediate variables of data we need to send the email
					string? email = viewModel.EmailData!.EmailAddress;
					string? subject = viewModel.EmailData!.EmailSubject;
					string? htmlMessage = viewModel.EmailData!.EmailBody;

					await _emailService.SendEmailAsync(email!, subject!, htmlMessage!);

					swalMessage = "Success: Email Sent!";
					return RedirectToAction(nameof(Index), new { swalMessage });
				}
				catch (Exception)
				{
                    swalMessage = "Error: Email Failed to Send.";
                    return RedirectToAction(nameof(Index), new { swalMessage });

                    throw;
				}
            }

			return View(viewModel);
		}

		// GET: Contacts/Create
		[HttpGet]
		public async Task<IActionResult> Create()
		{
			ViewData["CategoryList"] = await GetCategoriesListAsync();

			ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

			Contact contact = new Contact();

			return View(contact);
		}

		// POST: Contacts/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("FirstName,LastName,DateOfBirth,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,ImageFile")] Contact contact, IEnumerable<int> selected)
		{

			ModelState.Remove("AppUserId");

			if (ModelState.IsValid) //if all required info is met at the moment the info was sent over from form (and removing AppUserID for this logic method)
			{
				contact.AppUserId = _userManager.GetUserId(User);
				contact.CreatedDate = DateTime.UtcNow;

				if (contact.ImageFile != null)
				{
					contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
					contact.ImageType = contact.ImageFile.ContentType;
				}

				if (contact.DateOfBirth != null)
				{
					contact.DateOfBirth = DateTime.SpecifyKind(contact.DateOfBirth.Value, DateTimeKind.Utc);
				}

				_context.Add(contact); //_context is our db which can be modified; in this case Add the new contact
				await _context.SaveChangesAsync(); //saves changes to PostGres db

				//Add categories to contact
				await _addressBookService.AddCategoriesToContactAsync(selected, contact.Id);

				return RedirectToAction(nameof(Index)); //jump back to Index action (line 37 ish)
			}

			ViewData["CategoryList"] = GetCategoriesListAsync(selected);
			ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());
			return View(contact);
		}

		// GET: Contacts/Edit/5
		[HttpGet]
		public async Task<IActionResult> Edit(int? id)
		{
			//First check if id is null
			if (id == null || _context.Contacts == null)
			{
				return NotFound();
			}

			Contact? contact = await _context.Contacts
				                             .Include(c => c.Categories) //include categories already selected
											 .FirstOrDefaultAsync(c => c.Id == id);

			//Second check
			if (contact == null)
			{
				return NotFound();
			}

			ViewData["CategoryList"] = await GetCategoriesListAsync(contact.Categories.Select(c => c.Id));                                  
			ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

			return View(contact);
		}

		// POST: Contacts/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,CreatedDate,AppUserId,FirstName,LastName,DateOfBirth,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,ImageData,ImageType,ImageFile")] Contact contact, IEnumerable<int> selected)
		{
			if (id != contact.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					contact.CreatedDate = DateTime.SpecifyKind(contact.CreatedDate, DateTimeKind.Utc);

					if (contact.ImageFile != null)
					{
						contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
						contact.ImageType = contact.ImageFile.ContentType;
					}

					if (contact.DateOfBirth != null)
					{
						contact.DateOfBirth = DateTime.SpecifyKind(contact.DateOfBirth.Value, DateTimeKind.Utc);
					}

					_context.Update(contact);
					await _context.SaveChangesAsync();

					//Handle categories
					if (selected != null)
					{
						//remove current categories, update db, save changes
						await _addressBookService.RemoveCategoriesFromContactAsync(contact.Id);

						//add the updated categories; copied from Create
						await _addressBookService.AddCategoriesToContactAsync(selected, contact.Id);
					}
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ContactExists(contact.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}

			ViewData["CategoryList"] = await GetCategoriesListAsync(contact.Categories.Select(c => c.Id));
			ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());
			return View(contact);
		}

		// GET: Contacts/Delete/5
		[HttpGet]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Contacts == null)
			{
				return NotFound();
			}

			var contact = await _context.Contacts
				.Include(c => c.AppUser)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (contact == null)
			{
				return NotFound();
			}

			return View(contact);
		}

		// POST: Contacts/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Contacts == null)
			{
				return Problem("Entity set 'ApplicationDbContext.Contacts'  is null.");
			}
			var contact = await _context.Contacts.FindAsync(id);
			if (contact != null)
			{
				_context.Contacts.Remove(contact);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ContactExists(int id)
		{
			return (_context.Contacts?.Any(e => e.Id == id)).GetValueOrDefault();
		}

		private async Task<MultiSelectList> GetCategoriesListAsync(IEnumerable<int> categoryIds = null!)
		{
			string? appUserId = _userManager.GetUserId(User);

			IEnumerable<Category> categories = await _context.Categories
											  .Where(c => c.AppUserId == appUserId)
											  .ToListAsync();

			return new MultiSelectList(categories, "Id", "Name", categoryIds); //select tag needs this from cshtml file; using multiselect to be able to select multiple categories
		}
	}
}
