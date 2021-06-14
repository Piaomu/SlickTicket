using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Services.Interfaces
{
    public interface IImageService
    {
        Task<byte[]> EncodeImageAsync(IFormFile image);

        //encode an image from a url
        Task<byte[]> EncodeImageURLAsync(string imageURL);

        string DecodeImage(byte[] image, string contentType);

        string ContentType(IFormFile image);
    }
}
