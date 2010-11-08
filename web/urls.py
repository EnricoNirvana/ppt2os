from django.conf.urls.defaults import *
from django.conf import settings

# Uncomment the next two lines to enable the admin:
# from django.contrib import admin
# admin.autodiscover()

urlpatterns = patterns('',
(r'^$', 'upload.views.index'),
(r'^convert', 'upload.views.convert'),
(r'^upload', 'upload.views.upload'),
(r'^media/(?P<path>.*)$', 'django.views.static.serve',{'document_root': settings.STATIC_DOC_ROOT, 'show_indexes': True}),
)
