using Caching.Data;
using Caching.Models;
using Caching.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Caching.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly ILogger<DriversController> _logger;
        private readonly ICacheService _cacheService;
        private readonly AppDbContext _dbContext;

        public DriversController(ILogger<DriversController> logger, ICacheService cacheService, AppDbContext dbContext)
        {
            _logger = logger;
            _cacheService = cacheService;
            _dbContext = dbContext;
        }

        [HttpGet("all-drivers")]
        public async Task<IActionResult> Get()
        {
            //check cache data
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");
            if (cacheData != null && cacheData.Count() > 0)
            {
                return Ok(cacheData);
            }

            cacheData = await _dbContext.Drivers.ToListAsync();

            //set expiry time
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiryTime);
            return Ok(cacheData);
        }

        [HttpPost("add-driver")]
        public async Task<IActionResult> Post(Driver value)
        {            
            var addedObj = await _dbContext.Drivers.AddAsync(value);

            var expiryTime = DateTimeOffset.Now.AddSeconds(130);
            _cacheService.SetData<Driver>($"driver{value.Id}", addedObj.Entity, expiryTime);

            await _dbContext.SaveChangesAsync();

            return Ok(addedObj.Entity);
        }

        [HttpDelete("delete-driver")]
        public async Task<IActionResult> Ddelete(int id)
        {
            var exist = await _dbContext.Drivers.FirstOrDefaultAsync(x => x.Id == id);

            if (exist != null)
            {
                _dbContext.Remove(exist);
                _cacheService.RemoveData($"driver{id}");
                await _dbContext.SaveChangesAsync();
                return NoContent();
            }

            return NotFound();
        }

        [HttpGet("generate-pdf")]
        public async Task<IActionResult> GeneratePDF(string id)
        {
            var document = new PdfDocument();
            string htmlContent = "<h1>Welcome to dreamchasers</h1>";
            htmlContent += "<center><h1> EXPENSE LIST </h1> </center>";
            htmlContent += "<header>";
            htmlContent += "<input class='item' placeholder='Expense Item' type='text'>";
            htmlContent += "<input class='price' placeholder='Price' type='number'>";
            htmlContent += "<button>Add</button>";
            htmlContent += "</header>";
            htmlContent += "<div class='expenses'></div>";
            htmlContent += "<h2 style = 'color: brown;'> Total :<span class='total'></span></h2>";

            PdfGenerator.AddPdfPages(document, htmlContent, PageSize.A4);
            byte[]? response = null;
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms);
                response = ms.ToArray();
            }
            string fileName = "Invoice_" + id + ".pdf";
            return File(response, "application/pdf", fileName);
        }
        
    }
}
