using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YANENAVIZYETYLABY.Data;
using YANENAVIZYETYLABY.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;


namespace YANENAVIZYETYLABY.Controllers
{
    [Authorize]
    public class FoldersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FoldersController(ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var index = _context.Folders.Where(e => e.FolderId == null && e.ApplicationUserId == userId).ToList();

            return View(index);
        }

        public IActionResult Details(Guid? id)
        {
            if (id == null) return NotFound();

            var folder = _context.Folders.Include(e => e.Files).Include(e => e.Folders)
                .SingleOrDefault(e => e.Id == id);
            ViewBag.Path = GetPath(id);
            ViewBag.Can = GetCount(id);

            return View(folder);
        }

        public IActionResult Create(Guid? id)
        {
            if (id == null)
            {
                var model = new FolderV();
                return View(model);
            }
            else
            {
                var model = new FolderV { FolderId = id };

                ViewBag.Path = GetPath(id);
                return View(model);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FolderV model, Guid? id)
        {
            if (!ModelState.IsValid) return View(model);

            var folder = new Folder
            {
                FolderId = id,
                Name = model.Name,
                ApplicationUserId = _userManager.GetUserId(User)
            };
            _context.Add(folder);
            _context.SaveChanges();
            if (id == null) return RedirectToAction("Index");
            return RedirectToAction("Details", new { id });
        }
        public IActionResult Edit(Guid? id)
        {
            if (id == null) return NotFound();
            
            ViewBag.Id = id;
            return View(new FolderV());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FolderV model, Guid? id)

        {
            if (!ModelState.IsValid) return View(model);
            
            var folder = _context.Folders.SingleOrDefault(e => e.Id == id);
            folder.Name = model.Name;
            _context.SaveChanges();
            return folder.FolderId != null
                ? RedirectToAction("Details", new {id = folder.FolderId})
                : RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();
            var doc = _context.Folders.Include(e => e.Folders).Include(e => e.Files)
                .SingleOrDefault(e => e.Id == id);
            if (doc == null) return NotFound();
            _context.Folders.Remove(doc);
            await _context.SaveChangesAsync();
            return doc.FolderId != null
                ? RedirectToAction("Details", new { id = doc.FolderId })
                : RedirectToAction("Index");
        }


        private List<Tuple<Guid?, string>> GetPath(Guid? id)
        {
            var folder = _context.Folders.SingleOrDefault(e => e.Id == id);
            var root = new List<Tuple<Guid?, string>> {new Tuple<Guid?, string>(id, folder?.Name)};
            while (folder?.FolderId != null)
            {
                folder = _context.Folders.SingleOrDefault(e => e.Id == folder.FolderId);
                root.Add(new Tuple<Guid?, string>(folder?.Id, folder?.Name));
            }

            root.Reverse();
            return root;
        }

        private bool GetCount(Guid? id)
        {
            var folder = _context.Folders.SingleOrDefault(e => e.Id == id);
            var count = 0;
            while (folder?.FolderId != null)
            {
                folder = _context.Folders.SingleOrDefault(e => e.Id == folder.FolderId);
                count++;
            }
            return count == 0;
        }
    }
}