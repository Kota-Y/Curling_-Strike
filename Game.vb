Imports System.Math

Public Class Game
    Dim sHandle As Long = -1 ' サーバハンドル
    Dim cHandle As Long = -1 ' クライアントハンドル
    Dim enc As System.Text.Encoding = System.Text.Encoding.Default ' 文字コードに「Shift-JIS」を指定
    Public s(10) As Stone
    Dim centerline_x, setline_y As Integer
    Dim n, i, j, r2square As Integer
    Dim c_end As Integer = 0
    Dim t, dt As Double
    Dim all_v_flag As Boolean
    Dim gr As Graphics
    Dim accept_flag, turn_flag, mouse_flag As Boolean
    Dim ss_flag As Integer = -1
    Dim col_e As Double = 1 '反発係数
    Dim playerID, rivalID, playerscore, rivalscore As Integer
    Dim x, y, _x, _y As Integer
    Dim stop_flag As Boolean = False
    Public msg_flag As Boolean
    Dim ss_point As Integer = 0 'SSポイント
    Dim ss_shot_flag As Boolean = False
    Dim hide_flag As Boolean = False
    Dim p_score(3) As Integer  '自分のスコア
    Dim r_score(3) As Integer  '相手のスコア
    Dim p_totalscore As Integer = 0 '自分の総スコア 
    Dim r_totalscore As Integer = 0 '相手の総スコア

    Private Sub Game_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        gr = PictureBox1.CreateGraphics
        accept_flag = False
        mouse_flag = False
        turn_flag = False
        msg_flag = False
        Timer1.Interval = 10
        dt = 0.01
        centerline_x = PictureBox1.Width / 2
        setline_y = PictureBox1.Height / 1.1
        c_end = 0
        playerscore = 0
        rivalscore = 0
        r2square = (16 * 2) * (16 * 2)

        Button2.Text = "ストライクショット : OFF"
        Button2.Enabled = False
        radio_all()
    End Sub

    Private Sub Game_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Me.Location = Connect.Location
        Label3.Text = ss_point

        For i = 0 To 2
            p_score(i) = 0
            r_score(i) = 0
        Next

        Me.Size = New Size(720, 2000)

        Button3.Visible = False

        n = 0
        sHandle = Connect.sHandle
        cHandle = Connect.cHandle
        Label1.Text = sHandle
        'Dim i2 As Integer
        For i = 0 To 9
            s(i) = New Stone
            s(i).myu_set(0.1)
        Next

        If Connect.first_flag = True Then ' 先攻
            Label5.Text = "緑"
            playerID = 1
            rivalID = 2
            turn_flag = True
            If c_end = 0 Then
                data_ini()
            End If
            s(0).setID = playerID
        Else
            Label5.Text = "黄"
            playerID = 2
            rivalID = 1
            turn_flag = False
            If c_end = 0 Then
                data_ini()
            End If
            s(0).setID = rivalID
        End If

        court_draw()

        s(0).s_exist = True
        s(0).stone_set(0, 0, 0, 0)
        turn_show()
        If turn_flag = True Then
            accept_flag = True
        End If
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        'Dim i, j As Integer

        court_draw()
        For i = 0 To 9
            If (s(i).s_exist = True) Then
                s(i).timer(t, dt)
                s(i).out_check()

                '衝突判定
                Dim x_dis, y_dis As Integer
                For j = i + 1 To 9
                    If (s(j).s_exist = True) Then
                        x_dis = s(i).s_x - s(j).s_x
                        y_dis = s(i).s_y - s(j).s_y
                        If (x_dis * x_dis + y_dis * y_dis <= r2square) Then
                            collision(s(i), s(j), x_dis, y_dis)
                        End If
                    End If
                Next

                If s(i).s_x < 17 Then
                    s(i).s_xd = -s(i).s_xd
                ElseIf s(i).s_x > PictureBox1.Width - 15 Then
                    s(i).s_xd = -s(i).s_xd
                ElseIf s(i).s_y > PictureBox1.Height - 15 Then
                    s(i).s_yd = -s(i).s_yd
                End If
            End If
        Next

        all_v_flag = False
        For i = 0 To 9
            If (s(i).s_exist = True) Then
                If (s(i).check_v() = True) Then
                    all_v_flag = True
                End If
            End If
        Next

        If all_v_flag = False Then 'ターン終了
            Timer1.Enabled = False

            '発射地点に重なるときの補正
            For i = 0 To 9
                If s(i).s_exist = True Then
                    Dim x_dis, y_dis, dis, x, y As Double
                    x_dis = s(i).s_x - centerline_x
                    y_dis = s(i).s_y - setline_y
                    dis = x_dis * x_dis + y_dis * y_dis
                    If dis = 0 Then
                        dis = 1
                        y_dis = -1
                    End If
                    If dis < r2square Then
                        x = x_dis * Sqrt(r2square / dis)
                        y = y_dis * Sqrt(r2square / dis)
                        s(i).stone_delete()
                        s(i).s_x = centerline_x + x
                        s(i).s_y = setline_y + y
                        s(i).stone_draw()
                    End If
                    '補正後めり込んだとき再補正
                    For j = 0 To 9
                        If (s(j).s_exist = True And i <> j) Then
                            x_dis = s(i).s_x - s(j).s_x
                            y_dis = s(i).s_y - s(j).s_y
                            While x_dis * x_dis + y_dis * y_dis < r2square
                                x_dis = s(i).s_x - centerline_x
                                y_dis = s(i).s_y - setline_y
                                x = x_dis * 1.5
                                y = y_dis * 1.5
                                s(i).stone_delete()
                                s(i).s_x = centerline_x + x
                                s(i).s_y = setline_y + y
                                s(i).stone_draw()
                                x_dis = s(i).s_x - s(j).s_x
                                y_dis = s(i).s_y - s(j).s_y
                            End While

                        End If
                    Next
                End If
            Next

            If n < 9 Then '10投終わってないとき
                '次ターンへ
                n = n + 1
                '次ターンのための初期化処理
                col_e = 1
                stop_flag = False
                turn_flag = Not turn_flag
                turn_show()
                If turn_flag = True Then
                    s(n).setID = playerID
                    accept_flag = True
                Else
                    s(n).setID = rivalID
                End If

                '目くらまし
                If hide_flag = True And turn_flag = True Then
                    court_draw()
                End If

                hide_flag = False
                s(n).stone_set(0, 0, 0, 0)
            Else '10投終わったとき
                '点数計算
                Dim tmp_s As Stone
                x = centerline_x
                y = CType(PictureBox1.Height / 6, Integer)
                For i = 0 To 9
                    s(i).dis = 2147483640
                    If s(i).s_exist = True Then
                        s(i).dis = Pow(s(i).s_x - x, 2) + Pow(s(i).s_y - y, 2)
                    End If
                Next

                'ソート
                For i = 0 To 9 Step 1
                    For j = 9 To i + 1 Step -1
                        If s(j).dis < s(j - 1).dis Then
                            tmp_s = s(j)
                            s(j) = s(j - 1)
                            s(j - 1) = tmp_s

                        End If
                    Next
                Next

                If s(0).dis = 2147483640 Then
                Else
                    Dim score As Integer = 1
                    Dim win_id As Integer = s(0).getID
                    Dim i As Integer = 1
                    While win_id = s(i).getID And s(i).dis < 2147483640
                        score = score + 1
                        i = i + 1
                    End While

                    Dim show_end As Integer = c_end + 1

                    If win_id = playerID Then
                        playerscore = playerscore + score
                        p_score(c_end) = score
                        MessageBox.Show("あなたが " + score.ToString + " 点獲得しました。",
                                        "第" + show_end.ToString + "エンド終了",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.None)
                    ElseIf win_id = rivalID Then
                        rivalscore = rivalscore + score
                        r_score(c_end) = score
                        MessageBox.Show("相手が " + score.ToString + " 点獲得しました。",
                                        "第" + show_end.ToString + "エンド終了",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.None)
                    End If
                End If

                data_update()
                c_end = c_end + 1

                If c_end = 3 Then '3エンド終了後
                    p_totalscore = playerscore
                    r_totalscore = rivalscore
                    MessageBox.Show("結果は　あなた " + playerscore.ToString + " - " + rivalscore.ToString + " 相手　でした。",
                                   "試合終了",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.None)
                    Button3.Visible = True
                    'Connect.Close()
                    'Me.Close()
                Else '次のエンドへ
                    MessageBox.Show("現在の得点は　あなた " + playerscore.ToString + " - " + rivalscore.ToString + " 相手　です。" + vbCrLf + "次のエンドに移ります。",
                                    "得点発表",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.None)
                    msg_flag = True
                    Dim msg As String = "ok"
                    Connect.Send_msg(msg)
                End If
            End If
        End If
        t = t + dt              '　現在の時刻の更新
    End Sub

    Private Sub collision(ByVal s1 As Stone, ByVal s2 As Stone, ByVal x_dis As Double, ByVal y_dis As Double)
        If s1.check_v = False Then
            Dim s_tmp As New Stone
            s_tmp = s1
            s1 = s2
            s2 = s_tmp
        End If

        If s1.check_v = False Then
            Exit Sub
        End If

        If turn_flag = True And ss_flag = -1 Then
            ss_point = ss_point + 1
        End If

        Label3.Text = ss_point

        If ss_point >= 2 Then
            Button2.Enabled = True
        End If

        radio_all()
        If ss_point >= 7 Then
            RadioButton1.Enabled = True
            RadioButton2.Enabled = True
            RadioButton3.Enabled = True
            RadioButton4.Enabled = True
            RadioButton5.Enabled = True
        ElseIf ss_point >= 5 Then
            RadioButton1.Enabled = True
            RadioButton2.Enabled = True
            RadioButton5.Enabled = True
        ElseIf ss_point >= 3 Then
            RadioButton1.Enabled = True
            RadioButton5.Enabled = True
        ElseIf ss_point >= 2 Then
            RadioButton5.Enabled = True
        End If

        Dim speed2 As Double '速さ

        Dim s1_xd, s1_yd, s2_xd, s2_yd, n_x, n_y As Double

        s1_xd = s1.s_xd
        s1_yd = s1.s_yd
        s2_xd = s2.s_xd
        s2_yd = s2.s_yd
        n_x = x_dis / Sqrt(x_dis * x_dis + y_dis * y_dis)
        n_y = y_dis / Sqrt(x_dis * x_dis + y_dis * y_dis)

        '衝突の計算
        speed2 = (s1_xd * n_x + s1_yd * n_y + (s1_xd * n_x + s1_yd * n_y) * col_e) / 2
        s2_xd = speed2 * n_x
        s2_yd = speed2 * n_y
        s1_xd = s1_xd - speed2 * n_x
        s1_yd = s1_yd - speed2 * n_y

        s1.s_xd = s1_xd
        s1.s_yd = s1_yd
        s2.s_xd = s2_xd
        s2.s_yd = s2_yd

        '速度の設定後、衝突判定を満たさない位置まで進める処理
        While (x_dis * x_dis + y_dis * y_dis <= r2square)
            s1.timer(t, dt)
            s2.timer(t, dt)
            x_dis = s1.s_x - s2.s_x
            y_dis = s1.s_y - s2.s_y

            '追加
            If s1.check_v = False And s2.check_v = True Then
                s2.stone_set3(-1)
            End If

        End While
    End Sub

    Private Sub court_draw()
        gr.FillRectangle(Brushes.White, 0, 0, PictureBox1.Width, PictureBox1.Height)
        Dim BlueBrushes As New SolidBrush(Color.FromArgb(100, 0, 0, 255))
        Dim redBrushes As New SolidBrush(Color.FromArgb(100, 255, 0, 0))

        gr.FillEllipse(BlueBrushes, centerline_x - 100, CType(PictureBox1.Height / 6 - 100, Integer), 100 * 2, 100 * 2)
        gr.FillEllipse(Brushes.White, centerline_x - 75, CType(PictureBox1.Height / 6 - 75, Integer), 75 * 2, 75 * 2)
        gr.FillEllipse(redBrushes, centerline_x - 50, CType(PictureBox1.Height / 6 - 50, Integer), 50 * 2, 50 * 2)
        gr.FillEllipse(Brushes.White, centerline_x - 25, CType(PictureBox1.Height / 6 - 25, Integer), 25 * 2, 25 * 2)
    End Sub

    Private Sub PictureBox1_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseClick
        If stop_flag = True Then
            stop_stone()
            Dim msg As String
            msg = s(n).s_x.ToString
            msg = "sx_" + msg
            Connect.Send_msg(msg)
            System.Threading.Thread.Sleep(10)
            msg = s(n).s_y.ToString
            msg = "sy_" + msg
            Connect.Send_msg(msg)
            stop_flag = False
        End If
    End Sub

    Private Sub PictureBox1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseDown
        x = e.X
        y = e.Y
        _x = e.X
        _y = e.Y
        mouse_flag = True
    End Sub

    Private Sub PictureBox1_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseMove
        If mouse_flag = True And accept_flag = True Then
            gr.DrawLine(Pens.White, x, y, _x, _y)
            _x = e.X
            _y = e.Y
            gr.FillEllipse(Brushes.Black, x - 4, y - 4, 8, 8)
            gr.DrawLine(Pens.Black, x, y, _x, _y)
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Shell("Chat2010", AppWinStyle.NormalFocus)
        Connect.Close()
        Me.Close()
    End Sub

    Private Sub PictureBox1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseUp
        mouse_flag = False
        If accept_flag = True Then
            Dim v0, th, x_dis, y_dis As Integer
            Dim dig As Double
            x_dis = x - e.X
            y_dis = y - e.Y
            v0 = Sqrt(x_dis * x_dis + y_dis * y_dis) / 3
            dig = Atan2(-y_dis, x_dis)
            th = dig * 180 / PI - 90

            '引っ張るのが小さすぎた時発射しないように
            If v0 < 10 Then
                gr.FillEllipse(Brushes.White, x - 4, y - 4, 8, 8)
                gr.DrawLine(Pens.White, x, y, _x, _y)
                Exit Sub
            End If

            'ストライクショットボタンがONの時
            If ss_shot_flag = True And Button2.Enabled = True Then
                If RadioButton1.Checked = True Then
                    ss_flag = 1
                ElseIf RadioButton2.Checked = True Then
                    ss_flag = 2
                ElseIf RadioButton3.Checked = True Then
                    ss_flag = 3
                ElseIf RadioButton4.Checked = True Then
                    ss_flag = 4
                ElseIf RadioButton5.Checked = True Then
                    ss_flag = 5
                End If
            Else
                ss_flag = -1
            End If

            Dim msg As String
            msg = v0.ToString
            msg = "v0_" + msg
            Connect.Send_msg(msg)
            System.Threading.Thread.Sleep(100)
            msg = th.ToString
            msg = "th_" + msg
            Connect.Send_msg(msg)
            System.Threading.Thread.Sleep(100)
            msg = ss_flag.ToString
            msg = "ss_" + msg
            Connect.Send_msg(msg)
            shot_start(v0, th, ss_flag)
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Connect.Close()
        Me.Close()
    End Sub

    Private Sub turn_show()
        Label4.Text = "第" & c_end + 1 & "エンド : " & n + 1 & "投目"
        If turn_flag = True Then
            Label1.Text = "あなたの番です"
        Else
            Label1.Text = "相手の番です"
        End If
    End Sub

    '動作開始のメソッド
    Public Sub shot_start(ByVal v0 As Integer, ByVal th As Integer, ByVal ss As Integer)
        'Dim i As Integer
        ss_flag = ss
        'ストライクショット分岐

        If ss_flag = -1 Then
        ElseIf ss_flag = 1 Then
            v0 = v0 * 4
            Dim msg As String
            msg = "ef_"
            Connect.Send_msg(msg)
            ss_effect()
            If turn_flag = True Then
                ss_point = ss_point - 3
            End If
            ss_shot_flag = False
        ElseIf ss_flag = 2 Then
            col_e = 5
            Dim msg As String
            msg = "ef_"
            Connect.Send_msg(msg)
            ss_effect()
            If turn_flag = True Then
                ss_point = ss_point - 5
            End If
            ss_shot_flag = False
        ElseIf ss_flag = 3 Then
            stop_flag = True
            Dim msg As String
            msg = "ef_"
            Connect.Send_msg(msg)
            ss_effect()
            If turn_flag = True Then
                ss_point = ss_point - 7
            End If
            ss_shot_flag = False
        ElseIf ss_flag = 4 Then
            For i = 0 To n
                If s(i).s_exist = True And s(i).getID = s(n).getID Then
                    s(i).stone_set2(v0, th)
                End If
            Next
            Dim msg As String
            msg = "ef_"
            Connect.Send_msg(msg)
            ss_effect()
            If turn_flag = True Then
                ss_point = ss_point - 7
            End If
            ss_shot_flag = False
        ElseIf ss_flag = 5 Then
            hide_flag = True
            Dim msg As String
            msg = "ef_"
            Connect.Send_msg(msg)
            ss_effect()
            If turn_flag = True Then
                ss_point = ss_point - 2
            End If
            ss_shot_flag = False
        End If

        If ss_point >= 7 Then
            RadioButton1.Enabled = True
            RadioButton2.Enabled = True
            RadioButton3.Enabled = True
            RadioButton4.Enabled = True
            RadioButton5.Enabled = True
        ElseIf ss_point >= 5 Then
            RadioButton1.Enabled = True
            RadioButton2.Enabled = True
            RadioButton5.Enabled = True
        ElseIf ss_point >= 3 Then
            RadioButton1.Enabled = True
            RadioButton5.Enabled = True
        ElseIf ss_point >= 2 Then
            RadioButton5.Enabled = True
        End If

        Label3.Text = ss_point

        If ss_shot_flag = False Then
            Button2.Text = "ストライクショット : OFF"
            Button2.FlatStyle = FlatStyle.Standard
            Button2.BackColor = Color.White
        End If

        If ss_point < 2 Then
            Button2.Enabled = False
        End If

        s(n).s_exist = True
        s(n).stone_set(0, 0, v0, th)
        t = 0
        For i = 0 To 9
            If (s(i).s_exist = True) Then
                s(i).stone_draw()
            End If
        Next

        Timer1.Enabled = True
        accept_flag = False
    End Sub

    Public Sub stop_stone()
        s(n).s_xd = 0
        s(n).s_yd = 0
    End Sub

    Public Sub stop_stone_set(ByVal _x1 As Integer, ByVal _y1 As Integer)
        s(n).s_x = _x1
        s(n).s_y = _y1
    End Sub

    Public Sub end_change()
        court_draw()
        For i = 0 To 9
            s(i).s_exist = False
        Next
        n = 0
        '次ターンのための初期化処理
        col_e = 1
        stop_flag = False
        turn_show()
        If turn_flag = True Then
            s(n).setID = playerID
            accept_flag = True
        Else
            s(n).setID = rivalID
        End If
        s(n).stone_set(0, 0, 0, 0)
    End Sub

    'SSエフェクト
    Public Sub ss_effect()
        If ss_flag = 1 Then
            Label1.Text = "スピードアップ発動！"
        ElseIf ss_flag = 2 Then
            Label1.Text = "反発係数UP発動！"
        ElseIf ss_flag = 3 Then
            Label1.Text = "ストップショット発動！"
        ElseIf ss_flag = 4 Then
            Label1.Text = "大号令発動！"
        ElseIf ss_flag = 5 Then
            Label1.Text = "目くらまし発動！"
        End If
    End Sub

    'ラジオボタンを全効果無効
    Private Sub radio_all()
        RadioButton1.Enabled = False
        RadioButton2.Enabled = False
        RadioButton3.Enabled = False
        RadioButton4.Enabled = False
        RadioButton5.Enabled = False
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        ss_shot_flag = Not ss_shot_flag
        If ss_shot_flag = True Then
            Button2.Text = "ストライクショット : ON"
            Button2.FlatStyle = FlatStyle.System
            Button2.BackColor = Color.Red
        ElseIf ss_shot_flag = False Then
            Button2.Text = "ストライクショット : OFF"
            Button2.FlatStyle = FlatStyle.Standard
            Button2.BackColor = Color.White
        End If
    End Sub

    'スコアの初期設定
    Public Sub data_ini()
        'ヘッダの中央揃い
        DataGridView1.Columns(0).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridView1.Columns(1).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridView1.Columns(2).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridView1.Columns(3).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridView1.Columns(4).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter

        'データの中央揃い
        DataGridView1.Columns(0).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridView1.Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridView1.Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridView1.Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridView1.Columns(4).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

        '列の幅の設定
        DataGridView1.Columns(0).Width = 70
        DataGridView1.Columns(1).Width = 30
        DataGridView1.Columns(2).Width = 30
        DataGridView1.Columns(3).Width = 30
        DataGridView1.Columns(4).Width = 45

        If turn_flag = True Then
            DataGridView1.Rows.Add("あなた", "", "", "")
            DataGridView1.Rows.Add("相手", "", "", "")
        ElseIf turn_flag = False Then
            DataGridView1.Rows.Add("相手", "", "", "")
            DataGridView1.Rows.Add("あなた", "", "", "")
        End If

        Refresh()
    End Sub

    'スコアの更新
    Public Sub data_update()

        DataGridView1.Rows.Clear()      'すべてクリア

        Select Case c_end
            Case 0
                If playerID = 1 Then
                    DataGridView1.Rows.Add("あなた", p_score(0), "", "", playerscore)
                    DataGridView1.Rows.Add("相手", r_score(0), "", "", rivalscore)
                ElseIf playerID = 2 Then
                    DataGridView1.Rows.Add("相手", r_score(0), "", "", rivalscore)
                    DataGridView1.Rows.Add("あなた", p_score(0), "", "", playerscore)
                End If
            Case 1
                If playerID = 1 Then
                    DataGridView1.Rows.Add("あなた", p_score(0), p_score(1), "", playerscore)
                    DataGridView1.Rows.Add("相手", r_score(0), r_score(1), "", rivalscore)
                ElseIf playerID = 2 Then
                    DataGridView1.Rows.Add("相手", r_score(0), r_score(1), "", rivalscore)
                    DataGridView1.Rows.Add("あなた", p_score(0), p_score(1), "", playerscore)
                End If
            Case 2
                If playerID = 1 Then
                    DataGridView1.Rows.Add("あなた", p_score(0), p_score(1), p_score(2), playerscore)
                    DataGridView1.Rows.Add("相手", r_score(0), r_score(1), r_score(2), rivalscore)
                ElseIf playerID = 2 Then
                    DataGridView1.Rows.Add("相手", r_score(0), r_score(1), r_score(2), rivalscore)
                    DataGridView1.Rows.Add("あなた", p_score(0), p_score(1), p_score(2), playerscore)
                End If
        End Select
    End Sub

End Class