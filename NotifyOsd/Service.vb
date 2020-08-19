Imports System.Net.Sockets
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports NotifyOsd.Framework.Balloon

Public Class Service : Inherits HttpServer

    ReadOnly OsdNotifier As New OsdNotifier

    Public Sub New(port As Integer)
        MyBase.New(port, 2)
    End Sub

    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Call p.openResponseStream.WriteError(405, "GET method is not allowed!")
    End Sub

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)
        Select Case LCase(p.http_url)
            Case "/systemctl=stop"
                Call p.openResponseStream.WriteLine("notify-osd services has been shutdown!")
                Call Shutdown()
            Case "/systemctl=send_message"
                Dim request As New HttpPOSTRequest(p, inputData)
                Dim message As New Message With {
                    .behaviors = [Enum].Parse(GetType(BubbleBehaviors), request("behaviors")),
                    .content = request("content"),
                    .icon = request("icon"),
                    .sound = request("sound"),
                    .title = request("title")
                }

                Call OsdNotifier.SendMessage(message)
                Call p.openResponseStream.WriteLine("notify message task pending.")
            Case Else
                Call p.openResponseStream.WriteError(405, "invalid request!")
        End Select
    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Call p.openResponseStream.WriteError(405, "Unsure for the methods that you used...")
    End Sub

    Protected Overrides Function getHttpProcessor(client As TcpClient, bufferSize As Integer) As HttpProcessor
        Return New HttpProcessor(client, Me, bufferSize)
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        OsdNotifier.Dispose()
        Call MyBase.Dispose(disposing)
    End Sub
End Class
