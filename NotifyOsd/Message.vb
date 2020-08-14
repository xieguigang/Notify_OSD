Imports System.Drawing
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http

Public Class Message

    Public Property title As String
    Public Property content As String
    Public Property icon As String
    Public Property sound As String
    Public Property behaviors As BubbleBehaviors

    Public Function GetIconImage() As Image
        Return DataURI.URIParser(icon).ToStream.DoCall(AddressOf Image.FromStream)
    End Function
End Class

