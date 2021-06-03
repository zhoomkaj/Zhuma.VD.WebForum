using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YANENAVIZYETYLABY.Data;
using YANENAVIZYETYLABY.Models;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace YANENAVIZYETYLABY.Controllers
{
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public FilesController(ApplicationDbContext context,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Files.Include(f => f.Folder);
            return View(await applicationDbContext.ToListAsync());
        }

        public IActionResult Details(Guid? id)
        {
            if (id == null) return NotFound();

            var file = _context.Files
                .SingleOrDefault(m => m.Id == id);
            if (file == null) return NotFound();

            ViewBag.Folder = file.FolderId;
            return View(file);
        }


        public IActionResult Create(Guid? id)
        {
            ViewBag.Id = id;
            return View(new FileV());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Guid id, FileV model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Details", "Folders", new { id });


            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.FilePath.ContentDisposition)
                .FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);

            var file = new Models.File
            {
                Name = model.Name,
                Extension = fileExt,
                FolderId = id,
                Size = model.FilePath.Length
            };
            file.Name ??= fileName;

            var path = Path.Combine(_hostingEnvironment.WebRootPath, "files", file.Id.ToString("N") + fileExt);

            using (var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
            {
                model.FilePath.CopyTo(fileStream);
            }

            _context.Files.Add(file);
            _context.SaveChanges();
            return RedirectToAction("Details", "Folders", new { id });
        }


        public IActionResult Edit(Guid? id)
        {
            ViewBag.Id = id;
            return View(new FileV());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, FileV model)
        {
            if (!ModelState.IsValid) return View(model);

            var file = _context.Files.SingleOrDefault(e => e.Id == id);
            file.Name = model.Name;
            _context.SaveChanges();
            return RedirectToAction("Details", "Files", new { id });
        }

        public IActionResult Download(Guid id)
        {
            var file = _context.Files
                .SingleOrDefault(e => e.Id == id);
            if (file == null) return NotFound();

            var filesPath = Path.Combine(_hostingEnvironment.WebRootPath, "files",
                file.Id.ToString("N") + file.Extension);
            return PhysicalFile(filesPath, GetContentType(file.Extension), file.Name + file.Extension);
        }

        public string GetContentType(string extension)
        {
            var provider = new FileExtensionContentTypeProvider();
            return provider.TryGetContentType($"file.{extension}", out var result) ? result : "application/unknown";
        }
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();
            var file = await _context.Files.Include(e => e.Folder).SingleOrDefaultAsync(m => m.Id == id);
            _context.Files.Remove(file);
            if (file == null) return NotFound();
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Folders", new { id = file.FolderId });
        }
    }
}
