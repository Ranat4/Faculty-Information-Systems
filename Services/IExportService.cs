namespace FacultyInformationSystem_FIS_.Services
{
    // Generic Excel/PDF table export used by every review list page.
    public interface IExportService
    {
        byte[] BuildExcel(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows);

        byte[] BuildPdf(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows);
    }
}
