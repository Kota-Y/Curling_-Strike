Imports System.Math  'インポートの設定　以後はMath.CosなどをCosの様にMathなしで参照できる

Public Class Stone
    Const g = 9.8
    Const ep = 100
    Const r = 16

    Dim color As System.Drawing.Brush = Brushes.Black
    Dim v0, th, m As Integer
    Dim x, y, xd, yd As Double
    Dim exist, v_flag, coll_flag As Boolean
    Dim gr As Graphics
    Dim centerline_x, setline_y As Integer
    Dim f(10) As Double, u(10) As Double, ua(10) As Double, d_myu, s_myu As Double
    Dim NN As Integer, t#, dt#, x10#, x20#, l1#, th1#, th1d#, x1#, y1#, l2#, th2#, th2d#, x2#, y2#, K#, P#
    'Dim n As Integer
    Dim id As Integer
    Public dis As Integer


    'コンストラクタ
    Public Sub New()
        exist = False
        m = 15
        id = -1
        gr = Game.PictureBox1.CreateGraphics
        centerline_x = Game.PictureBox1.Width / 2
        setline_y = Game.PictureBox1.Height / 1.1
        NN = 4
        coll_flag = False
    End Sub

    Public ReadOnly Property s_r As Integer
        Get
            Return r
        End Get
    End Property

    Public Property s_exist As Boolean
        Get
            Return exist
        End Get
        Set(ByVal value As Boolean)
            exist = value
        End Set
    End Property

    Public Property s_coll_flag As Boolean
        Get
            Return coll_flag
        End Get
        Set(ByVal value As Boolean)
            coll_flag = value
        End Set
    End Property

    Public Property s_xd As Double
        Get
            Return xd
        End Get
        Set(ByVal value As Double)
            xd = value
        End Set
    End Property

    Public Property s_yd As Double
        Get
            Return yd
        End Get
        Set(ByVal value As Double)
            yd = value
        End Set
    End Property

    Public Property s_x As Double
        Get
            Return x
        End Get
        Set(ByVal value As Double)
            x = value
        End Set
    End Property

    Public Property s_y As Double
        Get
            Return y
        End Get
        Set(ByVal value As Double)
            y = value
        End Set
    End Property

    'セットボタンを押したときのストーンの配置
    Public Sub stone_set(ByVal _x As Integer, ByVal _y As Integer, ByVal _v0 As Integer, ByVal _th As Integer)
        x = centerline_x + _x
        y = setline_y - _y
        v0 = -_v0 * 10
        th = _th

        '色設定
        If id = 2 Then
            color = Brushes.Yellow
        ElseIf id = 1 Then
            color = Brushes.Green
        End If

        stone_draw()
        'shotline_draw()
        xd = v0 * Sin((Math.PI / 180) * th)
        yd = v0 * Cos((Math.PI / 180) * th)
    End Sub

    '初速と角度だけ設定
    Public Sub stone_set2(ByVal _v0 As Integer, ByVal _th As Integer)
        v0 = -_v0 * 10
        th = _th
        stone_draw()
        xd = v0 * Sin((Math.PI / 180) * th)
        yd = v0 * Cos((Math.PI / 180) * th)
    End Sub

    '初速だけ設定
    Public Sub stone_set3(ByVal _v0 As Integer)
        v0 = -_v0 * 10
        stone_draw()
    End Sub

    'ストーンの描写
    Public Sub stone_draw()
        gr.FillEllipse(color, CType(x - r, Integer), CType(y - r, Integer), r * 2, r * 2)
    End Sub

    'ストーンの軌道線の表示
    Public Sub shotline_draw()
        Dim x2, y2 As Integer
        x2 = CType(x + v0 * Sin((Math.PI / 180) * th) / 10, Integer)
        y2 = CType(y + v0 * Cos((Math.PI / 180) * th) / 10, Integer)
        gr.DrawLine(Pens.Red, CType(x, Integer), CType(y, Integer), x2, y2)
    End Sub

    'ストーンの描写
    Public Sub stone_delete()
        gr.FillEllipse(Brushes.White, CType(x - r, Integer), CType(y - r, Integer), r * 2, r * 2)
    End Sub

    'ストーンの軌道線の表示
    Public Sub shotline_delite()
        Dim x2, y2 As Integer
        x2 = CType(x + v0 * Sin((Math.PI / 180) * th) / 10, Integer)
        y2 = CType(y + v0 * Cos((Math.PI / 180) * th) / 10, Integer)
        gr.DrawLine(Pens.White, CType(x, Integer), CType(y, Integer), x2, y2)
    End Sub

    Public Sub myu_set(ByVal _dmyu As Double)
        d_myu = _dmyu
    End Sub

    Public Sub out_check()
        If y < 0 Then
            exist = False
            stone_delete()
        End If
    End Sub

    Public Function check_v() As Boolean
        If (xd = 0 And yd = 0) Then
            Return False
        Else
            Return True
        End If
    End Function

    'タイマー１ステップごとの処理
    Public Sub timer(ByVal _t As Double, ByVal _dt As Double)
        t = _t
        dt = _dt
        u(0) = x : u(1) = y : u(2) = xd : u(3) = yd   '　現在の値をuに設定
        RK_1step()                '　dt後の値を計算し，uに代入
        If (u(2) * u(2) + u(3) * u(3) < ep) Then
            u(2) = 0
            u(3) = 0
        End If
        x = u(0) : y = u(1) : xd = u(2) : yd = u(3)   '　uから新しい値を取得
        stone_draw()

        'n = n + 10
        'gr.FillEllipse(Brushes.Blue, n, n, 5, 5)
    End Sub

    '　dt後の値の計算（ルンゲ・クッタ法）
    '　　入力：u，出力：u，
    '　　使用するパラメータ：NN（１階の微分方程式の数）
    '　　補助関数：aux
    '　微分方程式の定義はauxで行うのでここは書き換える必要はない。

    Private Sub RK_1step()
        '　この関数のみで使う変数の型宣言
        Dim du1(10) As Double, du2(10) As Double, du3(10) As Double
        Dim du4(10) As Double
        Dim i%

        aux(u, t)
        For i = 0 To NN - 1 : du1(i) = f(i) * dt : Next i
        For i = 0 To NN - 1 : ua(i) = u(i) + du1(i) * 0.5 : Next i
        aux(ua, t + dt * 0.5)
        For i = 0 To NN - 1 : du2(i) = f(i) * dt : Next i
        For i = 0 To NN - 1 : ua(i) = u(i) + du2(i) * 0.5 : Next i
        aux(ua, t + dt * 0.5)
        For i = 0 To NN - 1 : du3(i) = f(i) * dt : Next i
        For i = 0 To NN - 1 : ua(i) = u(i) + du3(i) : Next i
        aux(ua, t + dt)
        For i = 0 To NN - 1 : du4(i) = f(i) * dt : Next i

        For i = 0 To NN - 1 : u(i) = u(i) + (du1(i) + (du2(i) + du3(i)) * 2.0# + du4(i)) / 6.0# : Next i
    End Sub

    '　aux : RK_1stepで使用する補助関数
    '　　配列fに微分方程式の右辺を表す式を代入する。
    '　　考えている微分方程式に基づき書き換える。

    Public Sub aux(ByVal u, ByVal t)
        '1階の微分方程式du(i)/dt=f(i)の定義 f(0)=...
        f(0) = u(2)
        f(1) = u(3)
        f(2) = -d_myu * g * u(2)
        f(3) = -d_myu * g * u(3)
    End Sub

    'Public Sub stone_color_set(ByVal col As Drawing.Brush)
    '    color = col
    'End Sub

    Public WriteOnly Property setID As Integer
        Set(ByVal value As Integer)
            id = value
        End Set
    End Property

    Public ReadOnly Property getID As Integer
        Get
            Return id
        End Get
    End Property

End Class
