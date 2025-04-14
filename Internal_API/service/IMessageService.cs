using Microsoft.AspNetCore.Mvc;

namespace Internal_API.service
{
    public interface IMessageService
    {
        Task SendMessage([FromBody] string fileid);
    }
}
