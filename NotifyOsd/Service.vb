Imports System.Net.Sockets
Imports Flute.Http.Core

Public Class Service : Inherits HttpServer

    Public Sub New(port As Integer, Optional threads As Integer = -1)
        MyBase.New(port, threads)
    End Sub

    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Call p.openResponseStream.WriteError(405, "GET method is not allowed!")
    End Sub

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)

    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Call p.openResponseStream.WriteError(405, "Unsure for the methods that you used...")
    End Sub

    Protected Overrides Function getHttpProcessor(client As TcpClient, bufferSize As Integer) As HttpProcessor
        Return New HttpProcessor(client, Me, bufferSize)
    End Function
End Class
