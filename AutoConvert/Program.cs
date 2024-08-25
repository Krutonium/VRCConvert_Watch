using System.Diagnostics;

namespace AutoConvert;

class Program
{
    static void Main(string[] args)
    {
        // Check if ffmpeg is available
        if (!ExistsOnPath("ffmpeg"))
        {
            Console.WriteLine("ffmpeg is not available on the system.");
            return;
        }
        // Create the input and output folders if they don't exist
        if (!Directory.Exists("input"))
            Directory.CreateDirectory("input");
        if (!Directory.Exists("output"))
            Directory.CreateDirectory("output");
        
        // Loop over the input folder every 30 seconds.
        // Get Core Count
        //ffmpeg -vaapi_device /dev/dri/renderD128 -i ./What\ was\ the\ N64\ Expansion\ Pak\ actually\ used\ for？\ \[YI4lBxTpzB4\].mp4 -vf 'format=nv12,hwupload' -c:v h264_vaapi -b:v 5M -maxrate 10M -bufsize 2M -c:a aac -b:a 192k -movflags faststart /media2/fileHost/gryphon/test_vrchat_amd.mp4
        
        var coreCount = Environment.ProcessorCount;
        while (true)
        {
            var files = Directory.GetFiles("input");
            foreach (var file in files)
            {
                // Check if the file is being written; if it increases in size after 5 seconds of waiting, skip it this time
                var size = new FileInfo(file).Length;
                Thread.Sleep(5000);
                if (size != new FileInfo(file).Length)
                {
                    Console.WriteLine($"Skipping {file} as it is being written.");
                    continue;
                }
                var output = Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".mp4");
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-vaapi_device /dev/dri/renderD128 -i \"{file}\" -vf 'format=nv12,hwupload' -c:v h264_vaapi -b:v 5M -maxrate 10M -bufsize 2M -c:a aac -b:a 192k -movflags faststart \"{output}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                //Also convert it to 2mbps 720p
                var output2 = Path.Combine("output", Path.GetFileNameWithoutExtension(file) + "_720p.mp4");
                var process2 = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-vaapi_device /dev/dri/renderD128 -i \"{file}\" -vf 'format=nv12,scale=1280:-1,hwupload' -c:v h264_vaapi -b:v 2M -maxrate 4M -bufsize 1M -c:a aac -b:a 192k -movflags faststart \"{output2}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process.WaitForExit();
                process2.WaitForExit();
                Console.WriteLine($"Converted {file} to {output}");
                //File.Delete(file);
                //Create a file with the URL's in it for the files
                // https://dl.krutonium.ca/gryphon/output/{output}
                
                File.AppendAllText($"{Path.GetFileNameWithoutExtension(file)}.txt", $"https://dl.krutonium.ca/gryphon/output/{Path.GetFileName(output)}\n");
                //File.AppendAllText($"{Path.GetFileNameWithoutExtension(file)}.txt", $"https://dl.krutonium.ca/gryphon/output/{Path.GetFileName(output2)}\n");
            }
            Console.WriteLine("Waiting for 30 seconds...");
            Thread.Sleep(30000);
        }
    }
    
    
    public static bool ExistsOnPath(string fileName)
    {
        return GetFullPath(fileName) != null;
    }

    public static string GetFullPath(string fileName)
    {
        if (File.Exists(fileName))
            return Path.GetFullPath(fileName);

        var values = Environment.GetEnvironmentVariable("PATH");
        foreach (var path in values.Split(Path.PathSeparator))
        {
            var fullPath = Path.Combine(path, fileName);
            if (File.Exists(fullPath))
                return fullPath;
        }
        return null;
    }
}