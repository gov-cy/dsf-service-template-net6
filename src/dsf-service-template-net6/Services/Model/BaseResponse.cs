namespace dsf_service_template_net6.Services.Model
{
    public class BaseResponse<T>
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = "";
        public T? Data { get; set; }
        public bool Succeeded { get; set; }
    }
}
