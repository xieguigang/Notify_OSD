Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/start")>
    <Usage("/start [/port <listen_port, default=3322>]")>
    Public Function Start(args As CommandLine) As Integer
        Dim port As Integer = args("/port") Or 3322
        Dim services As New NotifyOsd.Service(port)

        Return services.Run
    End Function

    <ExportAPI("/stop")>
    <Usage("/stop [/services <listen_port, default=3322>]")>
    Public Function [Stop](args As CommandLine) As Integer
        Dim port As Integer = args("/port") Or 3322
        Dim signal$ = $"http://127.0.0.1:{port}/systemctl=stop"
        Dim response As String = signal.POST

        Console.WriteLine(response)

        Return 0
    End Function
End Module
