Imports Microsoft.VisualBasic.CommandLine.Reflection

Module CLI

    <ExportAPI("-start", Usage:="-start service_id_url")>
    Public Function StartServices(argvs As CommandLine.CommandLine) As Integer
        Dim ServiceName As String = argvs.Parameters?.First
        Program.ProcessLock = New MMFProtocol.ProcessLock(ServiceName & "-process_lock")
        If Not ProcessLock.Locked Then
            Call DaemonProcess.Start(ServiceName)
        End If

        Return 0
    End Function

    <ExportAPI("-stop", Usage:="-stop service_id_url")>
    Public Function StopServices(argvs As CommandLine.CommandLine) As Integer
        Dim ServiceName As String = argvs.Parameters?.First
        Using Client As Microsoft.VisualBasic.MMFProtocol.MMFSocket = New MMFProtocol.MMFSocket(ServiceName)
            Call Client.SendMessage("-stop")
        End Using

        Return 0
    End Function

    <ExportAPI("-sendmessage", Usage:="-sendmessage -service <service_id_url> -message <messages>")>
    Public Function SendMessage(argvs As CommandLine.CommandLine) As Integer
        Dim ServiceName As String = argvs("-service")
        Dim s_Messages As String = argvs("-message")
        Using Client As Microsoft.VisualBasic.MMFProtocol.MMFSocket = New MMFProtocol.MMFSocket(ServiceName)
            Call Client.SendMessage(s_Messages)
        End Using

        Return 0
    End Function

    <ExportAPI("-about")>
    Public Function About() As Integer
        Dim AboutMsg As Message = New Message With {
            .Title = "Notify-osd service",
            .Message = $"Design by: xieguigang(xie.guigang@gmail.com)\nThis is a notify on screen dispalying service works like the 'notify-osd' on ubuntu linux.\n\n\n{DaemonProcess.Manual}"
        }
        Call DaemonProcess.SendMessage(AboutMsg)

        Return 0
    End Function
End Module
