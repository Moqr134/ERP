namespace Infrastructure.Logger;

public class Loger
{
    private const string path = @"Infrastructure\Logger\Loger.txt";
    public void Write(Exception ex, string msg)
    {
        try
        {
            string nl = Environment.NewLine;

            string hedar = msg + "\t" + DateTime.UtcNow.AddHours(3) + nl + nl;

            string text = ex != null ? ex.Message + nl : "";

            text += ex.InnerException != null ? ex.InnerException.Message + nl : "";

            string foter = nl + "---------------------------------------" + nl + nl;

            System.IO.File.AppendAllText(path, hedar + text + foter);
        }
        catch (Exception)
        {
            throw;
        }
    }


    public async Task WriteAsync(Exception ex, string msg)
    {
        try
        {
            string nl = Environment.NewLine;

            string hedar = msg + "\t" + DateTime.UtcNow.AddHours(3) + nl + nl;

            string text = ex != null ? ex.Message + nl : "";

            text += ex.InnerException != null ? ex.InnerException.Message + nl : "";

            string foter = nl + "---------------------------------------" + nl + nl;

            await System.IO.File.AppendAllTextAsync(path, hedar + text + foter);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
