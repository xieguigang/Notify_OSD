Imports Microsoft.VisualBasic.Parallel
Imports NotifyOsd.BubblesDisplay

Namespace Framework.Balloon

    Public Class ProgressBar

        Public Property PercentageValue As Integer
            Get
                Return Me._procBubble.Value
            End Get
            Set(value As Integer)
                Me._procBubble.Value = value
            End Set
        End Property

        Public ReadOnly Property ProgressBar As ProcessingBar
            Get
                Return Me._procBubble.ProcessingBar
            End Get
        End Property

        Public Sub Cancel()
            Call Me._procBubble.Cancel()
        End Sub

        Dim _invokeThread As Threading.Thread

        Public Sub ShowMessage(Percentage As Integer, MSG As Message)
            Me._procBubble.Value = Percentage
            Me._procBubble.Message = MSG
        End Sub

        Public Sub ShowMessage(Percentage As Integer, MSG As String)
            Me._procBubble.Value = Percentage
            Me._procBubble.Message = New Message With {
                .Message = MSG,
                .Title = Me._procBubble.Message.Title,
                .BubbleBehavior = Me._procBubble.Message.BubbleBehavior,
                .CallbackHandle = Me._procBubble.Message.CallbackHandle,
                .IconURL = Me._procBubble.Message.IconURL,
                .SoundURL = Me._procBubble.Message.SoundURL
            }
        End Sub

        Public Sub Show()
            If _invokeThread Is Nothing Then
                _invokeThread = RunTask(AddressOf Me._procBubble.ShowDialog)
            End If
        End Sub

        Dim _procBubble As FormOsdProgressIndicator

        Sub New(MSG As Message, ScreenOffset As Point)
            Me._procBubble = New FormOsdProgressIndicator()
            Me._procBubble.ScreenOffSet = ScreenOffset
            Me._procBubble.Message = MSG
        End Sub
    End Class
End Namespace