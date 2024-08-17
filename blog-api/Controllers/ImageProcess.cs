using AzureServiceBusDemo.Repositories;
using blog_api.Data;
using blog_api.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace blog_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageProcessController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IServiceBus _serviceBus;
        private readonly ICosmosService _cosmosService;
        public ImageProcessController(IBlobStorageService blobStorageService, 
            IServiceBus serviceBus,
            ICosmosService cosmosService)
        {
            this._blobStorageService = blobStorageService;
            this._serviceBus = serviceBus;
            _cosmosService = cosmosService;
        }
        // GET: api/<ImageProcess>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Image is uploaded successfully" };
        }

        // GET api/<ImageProcess>/5
        [HttpGet("{id}")]
        public int Get(int id)
        {
            return id+1;
        }
        //[HttpPost("20")]
        //public async Task<IActionResult> UploadImage(IFormFile image)
        //{
        //    if (image == null || image.Length == 0)
        //        return BadRequest("No file uploaded.");

        //    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", image.FileName);

        //    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await image.CopyToAsync(stream);
        //    }

        //    // Process the image if needed (e.g., save it to a database, perform analysis, etc.)

        //    return Ok(new { filePath });
        //}
        [HttpPost("upload-image")]
        
        public async Task<ActionResult> UploadImage(
           IFormFile formFile)
        {
            
            try
            {
                var id = Guid.NewGuid().ToString();
                
                
                if (formFile?.Length > 0)
                {

                    var url = await _blobStorageService.UploadBlob(formFile, formFile.FileName, id);
                    var newImage = new Image
                    {
                        FileName = formFile.FileName,
                        Url = url,
                        Id = id,
                        ImageContainer = "images",
                        Width = 300,
                        Height = 300
                    };
                    await  _serviceBus.SendMessageAsync(newImage);
                    await _cosmosService.UpsertImage(newImage);
                        
                }
                else
                {
                    return NotFound();
                }


                return Ok();

                //return RedirectToAction(nameof(Index));
            }
            catch
            {
                return NotFound();
            }
        }
        // POST api/<ImageProcess>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] string name,
           IFormFile formFile)
        {

            Image image = new Image();
            try
            {
                var id = Guid.NewGuid().ToString();
                image.Id = id;

                if (formFile?.Length > 0)
                {

                    await _blobStorageService.UploadBlob(formFile, name, id);
                }
                else
                {
                    return NotFound();
                }


                return Ok();

                //return RedirectToAction(nameof(Index));
            }
            catch
            {
                return NotFound();
            }
        }

        // PUT api/<ImageProcess>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ImageProcess>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
