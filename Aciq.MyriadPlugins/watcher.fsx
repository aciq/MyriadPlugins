
open System.Diagnostics
open System.IO
type Process with
    static member runToStdout(pname,pargs,cwd) = 
        use p = new Process(StartInfo= ProcessStartInfo(fileName=pname,arguments=pargs,WorkingDirectory=cwd))
        p.Start() |> ignore
        p.WaitForExit()

open System

let watcher = new System.IO.FileSystemWatcher(".","*.fs",EnableRaisingEvents=true , IncludeSubdirectories=true )
let [<Literal>] TestProjectPath =
    __SOURCE_DIRECTORY__
    + "/../Aciq.MyriadPlugins.Test"

let fp (x,y) = Path.GetFullPath(x,y)
let onChanged() =
    stdout.WriteLine "running "
    let args =
        [
            let userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            $@"{userFolder}/.nuget/packages/myriad.sdk/0.8.1/tools/net6.0/any/Myriad.dll"
            "--inputfile"
            fp("Sample1.fs",TestProjectPath)
//            "--outputfile"
//            fp("Sample1Gen.fs",TestProjectPath)
            
            "--configfile"
            fp("myriad.toml",TestProjectPath)
            "--plugin"
            $@"{__SOURCE_DIRECTORY__}/bin/Debug/net6.0/Aciq.MyriadPlugins.dll"
            "--inlinegeneration"
//            "--configkey"
//            "conf1"
        ]
        |> String.concat " "
    stdout.WriteLine args
    Process.runToStdout("dotnet","build -c Debug", __SOURCE_DIRECTORY__)
    Process.runToStdout("dotnet", args, __SOURCE_DIRECTORY__)

let rec loop (nextproctime:DateTimeOffset) : unit =
    let _ = watcher.WaitForChanged(System.IO.WatcherChangeTypes.Changed)
    match DateTimeOffset.Now > nextproctime with 
    | false -> 
        loop (nextproctime) 
    | true -> 
        onChanged()  
        loop (nextproctime.AddSeconds(20))   
        
loop DateTimeOffset.Now