
Public Class ValueAdjustments

    Public Property PercentageValue As Integer
        Get
            Return Me._AdjustmentBar.Value
        End Get
        Set(value As Integer)
            Me._AdjustmentBar.Value = value
        End Set
    End Property

    Dim _AdjustmentBar As FormOsdValueAdjustments

    Dim _InvokeThread As Threading.Thread

    Public Sub Show()
        If _InvokeThread Is Nothing Then
            Me._InvokeThread = New Threading.Thread(AddressOf Me._AdjustmentBar.ShowDialog)
            Call Me._InvokeThread.Start()
        End If
    End Sub

    Public ReadOnly Property ProcessingBar As ProcessingBar
        Get
            Return Me._AdjustmentBar.ProcessingBar
        End Get
    End Property

#Region "Constructors"

    Sub New(MSG As Message, Up As ValueAdjustmentInvoke, Down As ValueAdjustmentInvoke, ValueChanged As ValueAdjustmentInvoke)
        Me._AdjustmentBar = New FormOsdValueAdjustments(Up, Down, ValueChanged)
        Me._AdjustmentBar.Message = MSG
    End Sub

    Sub New(MSG As Message, Up As ValueAdjustmentInvoke, Down As ValueAdjustmentInvoke)
        Me._AdjustmentBar = New FormOsdValueAdjustments(Up, Down)
        Me._AdjustmentBar.Message = MSG
    End Sub

    Sub New(MSG As Message, ValueChanged As ValueAdjustmentInvoke)
        Me._AdjustmentBar = New FormOsdValueAdjustments(ValueChanged)
        Me._AdjustmentBar.Message = MSG
    End Sub
#End Region
End Class