using ewms_common.Infras.Enums;

namespace ewms_common.Wrappers
{
    public class ImportData<T>
    {
        public ImportData() { }
        public ImportData(T data)
        {
            this.data = data;
            status = eStatus_Import.Valid;
        }
        public T? data { get; set; }
        public eStatus_Import status { get; set; }
        public List<ImportDataError> errors { get; set; } = new List<ImportDataError>();
    }

    public class ImportDataError
    {
        public string name { get; set; } = null!;
        public string message { get; set; } = null!;
    }
}
