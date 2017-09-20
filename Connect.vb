Imports System.Net

Public Class Connect
    Const port As Integer = 20000 ' ポート番号
    Public sHandle As Long = -1 ' サーバハンドル
    Public cHandle As Long = -1 ' クライアントハンドル
    Dim enc As System.Text.Encoding = System.Text.Encoding.Default ' 文字コードに「Shift-JIS」を指定
    Dim v0, th As Integer
    Dim front_flag As Boolean = True    'コイントスの表裏(デフォルトで表)
    Dim random_flag As Boolean = True   '乱数の偶奇数(デフォルトで奇数)
    Dim image_count As Integer = 0
    Dim playercoin, rivalcoin As String '自分と相手のコインの状態の文字
    Public first_flag As Boolean
    Dim s_x, s_y As Double

    ' 「待機」ボタンが押された
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ' リッスンおよび接続が行われていないとき
        If sHandle = -1 AndAlso cHandle = -1 Then
            Try
                sHandle = TcpSockets1.OpenAsServer(port) ' サーバとして動作
                Label1.Text = "待機中!"
            Catch ex As Exception ' 例外がスローされたとき
                ' エラーメッセージを出力
                Label1.Text = "エラー発生!"
            End Try
        End If
    End Sub

    ' 「接続」ボタンが押された
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        ' リッスンおよび接続が行われていないとき
        If sHandle = -1 AndAlso cHandle = -1 Then
            Try
                Dim host As String = TextBox1.Text
                ' クライアントとして動作
                cHandle = TcpSockets1.OpenAsClient(host, port)
                Label1.Text = "接続中 !"
            Catch ex As Exception ' 例外がスローされたとき
                ' エラーメッセージを出力
                Label1.Text = "エラー発生!"
            End Try
        End If
    End Sub

    ' 「切断」ボタンが押された
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If sHandle <> -1 Then ' サーバとして動作しているとき
            TcpSockets1.Close(sHandle) ' リッスンをやめる
            sHandle = -1
            Label1.Text = "待機終了 !"
        End If
        If cHandle <> -1 Then ' クライアントとして動作しているとき
            TcpSockets1.Close(cHandle) ' リモートとの通信を切断する
            Label1.Text = "切断 !"
        End If
    End Sub

    ' Acceptイベントが発生した
    Private Sub TcpSockets1_Accept(ByVal sender As System.Object, ByVal e As Experiment.TcpSocket.AcceptEventArgs) Handles TcpSockets1.Accept
        cHandle = e.ClientHandle
        Label1.Text = "受付 !"
    End Sub

    ' Connectイベントが発生した
    Private Sub TcpSockets1_Connect(ByVal sender As System.Object, ByVal e As Experiment.TcpSocket.ConnectEventArgs) Handles TcpSockets1.Connect
        Label1.Text = "接続完了 !"

        If sHandle <> -1 Then ' サーバとして動作しているとき
            Label1.Text = "先攻後攻を決めます。" + vbCrLf + "コインの裏表を選んでください。"
            Button6.Visible = True
            GroupBox1.Visible = True
            RadioButton1.Visible = True
            RadioButton2.Visible = True
        Else
            Label1.Text = "先攻後攻を決めまています。" + vbCrLf + "親の選択を待ってください。"
        End If
    End Sub

    ' Disconnectイベントが発生した
    Private Sub TcpSockets1_Disconnect(ByVal sender As System.Object, ByVal e As Experiment.TcpSocket.DisconnectEventArgs) Handles TcpSockets1.Disconnect
        cHandle = -1
        Label1.Text = "切断されました !"
    End Sub

    ' DataReceiveイベントが発生した
    Private Sub TcpSockets1_DataReceive(ByVal sender As System.Object, ByVal e As Experiment.TcpSocket.DataReceiveEventArgs) Handles TcpSockets1.DataReceive
        Dim msg As String = enc.GetString(e.Data) ' バイト配列を文字列に変換

        If InStr(msg, "v0_") = 1 Then
            v0 = Integer.Parse(Mid(msg, 4))
        ElseIf InStr(msg, "th_") = 1 Then
            th = Integer.Parse(Mid(msg, 4))
        ElseIf InStr(msg, "ss_") = 1 Then
            Dim ss As Integer
            ss = Integer.Parse(Mid(msg, 4))
            Game.shot_start(v0, th, ss)
        ElseIf InStr(msg, "sx_") = 1 Then
            Game.stop_stone()
            s_x = Double.Parse(Mid(msg, 4))
        ElseIf InStr(msg, "sy_") = 1 Then
            s_y = Double.Parse(Mid(msg, 4))
            Game.stop_stone_set(s_x, s_y)
        ElseIf InStr(msg, "ok") = 1 Then
            If Game.msg_flag = True Then
                Game.end_change()
                msg = "rep_ok"
                Send_msg(msg)
            End If
        ElseIf InStr(msg, "rep_ok") = 1 Then
            Game.end_change()
        ElseIf InStr(msg, "co_") = 1 Then
            rivalcoin = Mid(msg, 4)
            If rivalcoin = "表" Then
                playercoin = "裏"
                front_flag = False
            Else
                playercoin = "表"
                front_flag = True
            End If
        ElseIf InStr(msg, "ra_") = 1 Then
            Dim check As Integer
            check = Mid(msg, 4)
            decided(check)
        ElseIf InStr(msg, "ef_") = 1 Then
            Game.ss_effect()
        End If
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Me.Close()
    End Sub

    Private Sub Connect_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        '自分のコンピュータ名を取得する
        Dim HostName As String
        HostName = System.Net.Dns.GetHostName()

        'IPアドレスのリストを取得
        Dim ipHE As IPHostEntry
        ipHE = Dns.GetHostEntry(HostName)

        'IPアドレスを取得する
        Dim ip As IPAddress
        ip = ipHE.AddressList(1)

        TextBox1.Text = ip.ToString
        Label1.Text = ""
        Label2.Text = ""
        Me.Location = New Point(0, 0)

        PictureBox1.ImageLocation = get_path() + "\new_c1.jpg"
        PictureBox2.ImageLocation = get_path() + "\new_c2.jpg"
        PictureBox3.ImageLocation = get_path() + "\new_cointosu.jpg"
        PictureBox4.ImageLocation = get_path() + "\new_cointosu1.jpg"
    End Sub

    Public ReadOnly Property GetsHandle As Integer
        Get
            Return sHandle
        End Get
    End Property

    Public ReadOnly Property GetcHandle As Integer
        Get
            Return cHandle
        End Get
    End Property

    '送信メソッド
    Public Sub Send_msg(ByVal msg As String)
        Dim bytes As Byte() = enc.GetBytes(msg) ' 文字列をバイト配列に変換
        TcpSockets1.Send(cHandle, bytes) ' メッセージ送信
    End Sub


    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        Shell("Chat2010", AppWinStyle.NormalFocus)
        Me.Location = New Point(700, 0)
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        Dim random_number As New System.Random()
        Dim check_number As Integer = random_number.Next()  '0以上の乱数を生成

        If RadioButton1.Checked = True Then     '予想が表の時
            front_flag = True
            playercoin = "表"
            rivalcoin = "裏"
            Dim msg As String
            msg = "co_表"
            Send_msg(msg)
            msg = check_number.ToString
            msg = "ra_" + msg
            Send_msg(msg)

        ElseIf RadioButton2.Checked = True Then '予想が裏の時
            front_flag = False
            playercoin = "裏"
            rivalcoin = "表"
            Dim msg As String
            msg = "co_裏"
            Send_msg(msg)
            msg = check_number.ToString
            msg = "ra_" + msg
            Send_msg(msg)
        End If

        decided(check_number)
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Select Case image_count
            Case 0, 1
                image3()
            Case 2, 3, 4
                image4()
            Case 5
                If random_flag = True Then      '乱数が奇数の時、表を表示
                    image2()
                ElseIf random_flag = False Then '乱数が偶数の時、裏を表示
                    image1()
                End If
        End Select

        image_count = image_count + 1

        If image_count = 6 Then
            image_count = 0
            judge()
            'Button2.Enabled = True
            Timer1.Enabled = False
            Me.Refresh()
            System.Threading.Thread.Sleep(2000)
            Game.Show()
            Game.Refresh()
            Me.Hide()
        End If
    End Sub

    'コイントスの勝敗を判定
    Private Sub judge()
        If random_flag = True And front_flag = True Then
            Label1.Text = "当たりです！" & vbCrLf & "あなたは先攻です！"
            first_flag = True
        ElseIf random_flag = True And front_flag = False Then
            Label1.Text = "はずれです！" & vbCrLf & "あなたは後攻です！"
            first_flag = False
        ElseIf random_flag = False And front_flag = False Then
            Label1.Text = "当たりです！" & vbCrLf & "あなたは先攻です！"
            first_flag = True
        ElseIf random_flag = False And front_flag = True Then
            Label1.Text = "はずれです！" & vbCrLf & "あなたは後攻です！"
            first_flag = False
        End If
    End Sub

    '裏画像のみ表示
    Private Sub image1()
        PictureBox1.Visible = True
        PictureBox2.Visible = False
        PictureBox3.Visible = False
        PictureBox4.Visible = False
        Label2.Text = "裏"
    End Sub

    '表画像のみ表示
    Private Sub image2()
        PictureBox1.Visible = False
        PictureBox2.Visible = True
        PictureBox3.Visible = False
        PictureBox4.Visible = False
        Label2.Text = "表"
    End Sub

    'コイントスの流れ1のみ表示
    Private Sub image3()
        PictureBox1.Visible = False
        PictureBox2.Visible = False
        PictureBox3.Visible = True
        PictureBox4.Visible = False
    End Sub

    'コイントスの流れ2のみ表示
    Private Sub image4()
        PictureBox1.Visible = False
        PictureBox2.Visible = False
        PictureBox3.Visible = False
        PictureBox4.Visible = True
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    '画像を全非表示
    Private Sub imageall()
        PictureBox1.Visible = False
        PictureBox2.Visible = False
        PictureBox3.Visible = False
        PictureBox4.Visible = False
    End Sub

    '実行ファイルの絶対パスを取得
    Public Shared Function get_path() As String
        Return System.IO.Path.GetDirectoryName( _
        System.Reflection.Assembly.GetExecutingAssembly().Location)
    End Function

    Public Sub decided(ByVal check_number As Integer)
        If sHandle <> -1 Then ' サーバとして動作しているとき
            Label1.Text = playercoin + "を選択しました。"
        Else
            Label1.Text = "親が" + rivalcoin + "を選択しました。" + vbCrLf + "あなたは" + playercoin + "です。"
        End If

        If check_number Mod 2 Then  '乱数が奇数の時
            random_flag = True
        Else    '乱数が偶数の時
            random_flag = False
        End If

        imageall()

        Label2.Text = ""
        Timer1.Enabled = True
        Button6.Enabled = False
    End Sub

End Class
