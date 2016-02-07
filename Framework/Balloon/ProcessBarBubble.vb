
Public Class ProcessBarBubble

    Public Property PercentageValue As Integer
        Get
            Return Me._ProcBubble.Value
        End Get
        Set(value As Integer)
            Me._ProcBubble.Value = value
        End Set
    End Property

    Public ReadOnly Property ProcessingBar As ProcessingBar
        Get
            Return Me._ProcBubble.ProcessingBar
        End Get
    End Property

    Public Sub Cancel()
        Call Me._ProcBubble.Cancel()
    End Sub

    Dim _InvokeThread As Threading.Thread

    Public Sub ShowMessage(Percentage As Integer, MSG As Message)
        Me._ProcBubble.Value = Percentage
        Me._ProcBubble.Message = MSG
    End Sub

    Public Sub ShowMessage(Percentage As Integer, MSG As String)
        Me._ProcBubble.Value = Percentage
        Me._ProcBubble.Message = New Message With {.Message = MSG, .Title = Me._ProcBubble.Message.Title,
            .BubbleBehavior = Me._ProcBubble.Message.BubbleBehavior,
            .CallbackHandle = Me._ProcBubble.Message.CallbackHandle,
            .IconURL = Me._ProcBubble.Message.IconURL,
            .SoundURL = Me._ProcBubble.Message.SoundURL}
    End Sub

    Public Sub Show()
        If _InvokeThread Is Nothing Then
            Me._InvokeThread = New Threading.Thread(AddressOf Me._ProcBubble.ShowDialog)
            Call Me._InvokeThread.Start()
        End If
    End Sub

    Dim _ProcBubble As FormOsdProcessIndicator

    Sub New(MSG As Message, ScreenOffset As Point)
        Me._ProcBubble = New FormOsdProcessIndicator()
        Me._ProcBubble.ScreenOffSet = ScreenOffset
        Me._ProcBubble.Message = MSG

    End Sub

End Class
