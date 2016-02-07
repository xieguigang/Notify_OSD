Public Module Form1


    Dim nnn As NotifyOsd.Framework.Balloon.ProgressBar

    Sub Main()

        nnn = New NotifyOsd.Framework.Balloon.ProgressBar(New NotifyOsd.Message With {.Title = "测试消息头333333", .Message = "44444收到甲方监理考试的\n恢复健康绿色的很快就就考虑实际发生\n444444444\ngjgfdgdfjljkldssdsddd\nhkhhhhhhhhhhhhhhhhh\nhhhhhhhhhhhhhhhhhhhh"}, New Point)
        Call nnn.Show()

        nnn.ProgressBar.AnimationSpeed = 995

        For i As Integer = 1 To 100

            nnn.PercentageValue = i
            Call Threading.Thread.Sleep(300)

        Next


        Dim dddd As New NotifyOsd.Framework.Balloon.ValueAdjuster(New NotifyOsd.Message With {.Title = "sfsdfsdfsdf", .Message = "sadddddddddddddddddddddddddddddddd\ndddddddddddddddddddddddd"},
                                                   Sub(value) Console.WriteLine($"{NameOf(value)} ===> {value }"))

        Call dddd.Show()
        dddd.PercentageValue = 50
        Call Console.ReadLine()


        Call New NotifyOsd.Framework.Balloon.OsdNotifier().SendMessage("test ok!", "test ok!", "", Nothing)
    End Sub
End Module
