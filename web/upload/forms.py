from django import forms

class UploadFileForm(forms.Form):
    first_name = forms.CharField()
    last_name = forms.CharField()
    password = forms.CharField(widget=forms.PasswordInput)
