@Include=SharedVariables.inc

[TitleStyle]
StringAlign=Center
StringCase=Upper
StringStyle=Bold
StringEffect=Shadow
FontEffectColor=0,0,0,50
FontColor=#fontColor#
FontFace=#fontFace#
FontSize=#titleFontSize#
AntiAlias=1
ClipString=1
X=(#w# / 2)
Y=(#h# / 2)
W=#w#
H=#h#

[ContainerStyle]
Shape=Rectangle 0,0,#w#,#h#
X=15
Y=(#h# * 3 / 2)r
W=(#w# - 30)
H=#h#

[LabelStyle]
StringAlign=Left
StringCase=None
StringStyle=Bold
StringEffect=Shadow
FontEffectColor=0,0,0,20
FontColor=#fontColor#
FontFace=#fontFace#
FontSize=#fontSize#
AntiAlias=1
ClipString=1
W=(#w# - 30)
H=#h#

[ValueStyle]
StringAlign=Right
StringCase=None
StringStyle=Bold
StringEffect=Shadow
FontEffectColor=0,0,0,20
FontColor=#fontColor#
FontFace=#fontFace#
FontSize=#fontSize#
AntiAlias=1
ClipString=1
X=(#w# - 30)
W=(#w# - 30)
H=#h#

[SeperatorStyle]
SolidColor=255,255,255,15
Y=(#h# - 1)
W=(#w# - 30)
H=1
