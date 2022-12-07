namespace dsf_service_template_net6.Services.Model
{

 
    public class TasksGetResponse
    {
        public int errorCode { get; set; }
        public string errorMessage { get; set; }
        public Task[] data { get; set; }
        public bool succeeded { get; set; }
        public string informationMessage { get; set; }
    }
    public class TasksPostResponse
    {
        public int errorCode { get; set; }
        public string errorMessage { get; set; }
        public Task data { get; set; }
        public bool succeeded { get; set; }
        public string informationMessage { get; set; }
    }

    public class Task
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool isComplete { get; set; }
    }

}
