Imports System.Net.Sockets
Imports Flute.Http.Core

Public Class Service : Inherits HttpServer

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
End Class
