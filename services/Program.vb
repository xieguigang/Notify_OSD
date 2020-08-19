Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON
Imports NotifyOsd

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

    <ExportAPI("/send")>
    <Usage("/send /title <title> /message <message> /icon <file.png> [/services <listen_port, default=3322>]")>
    Public Function sendMessage(args As CommandLine) As Integer
        Dim title$ = args <= "/title"
        Dim message$ = args <= "/message"
        Dim icon$ = args <= "/icon"
        Dim port As Integer = args("/port") Or 3322

        icon = New DataURI(icon).ToString

        With New MultipartForm()
            Call .Add(NameOf(NotifyOsd.Message.behaviors), BubbleBehaviors.AutoClose.ToString)
            Call .Add(NameOf(NotifyOsd.Message.icon), icon)
            Call .Add(NameOf(NotifyOsd.Message.title), title)
            Call .Add(NameOf(NotifyOsd.Message.message), message)

            Call Console.WriteLine(.POST($"http://127.0.0.1:{port}/systemctl=send_message"))
        End With

        Return 0
    End Function
End Module
