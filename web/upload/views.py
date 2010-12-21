from django.shortcuts import get_object_or_404, render_to_response
from django.http import HttpResponseRedirect, HttpResponse
from django.core.urlresolvers import reverse
from django.template import RequestContext
import os
from forms import *
import xmlrpclib
import time
import simplejson as json
from ConfigParser import ConfigParser
from django.conf import settings

ppt2os_config = ConfigParser()
ppt2os_config.read(os.path.join(settings.SERVICE_ROOT,'../','ppt2os.ini'))

def index(request):
    #fase 1: sla de ppt op
    if request.method == 'POST':
        #stap 1: schrijf het bestand weg
        file_size = request.META['HTTP_X_FILE_SIZE']
        file_name = request.META['HTTP_X_FILE_NAME']
        destination = open(os.path.join("../process",file_name), 'wb+')
        destination.write(request.raw_post_data)
        destination.close()
        response = {'status':{'code':'FILE_UPLOADED','feedback':'File(s) succesfully uploaded'}}
        return HttpResponse(json.dumps(response), mimetype='application/json')  
    else:
        form = UploadFileForm()
    return render_to_response('index.html', {},context_instance=RequestContext(request))
    

def convert(request):
    ppts = request.POST.getlist('names[]')
    #stap 2: geef de powerpoint service opdracht om te converteren
    ppt_proxy = xmlrpclib.ServerProxy("http://127.0.0.1:%s" % ppt2os_config.getint("converter","port"))
    total_slides = {}
    for ppt in ppts: 
        total_slides[ppt] = ppt_proxy.ppt2os.ppt_to_png(ppt,'dirname','jpg')        
    response = {'status':{'code':'FILE_CONVERTED','feedback':'File(s) succesfully converted'},'data':{'slides':total_slides}}
    return HttpResponse(json.dumps(response), mimetype='application/json')  
    
    
def upload(request):
    slides = request.POST.getlist('names[]')
    firstname = request.POST.__getitem__('firstname')
    lastname = request.POST.__getitem__('lastname')
    password = request.POST.__getitem__('password')
    os_proxy = xmlrpclib.ServerProxy("http://127.0.0.1:%s" % ppt2os_config.getint("uploader","port"))
    logged_in = os_proxy.exposed.login(firstname,lastname,password,ppt2os_config.get("web","user_server"))
    if logged_in == True:
        while True:
            try:
                time.sleep(1)
                uuids = os_proxy.exposed.upload_textures(slides)
                break
            except xmlrpclib.Fault:
                pass
    else:
        print "LOGIN FAILED"
    response = {'status':{'code':'FILE_UPLOADED','feedback':'File(s) succesfully uploaded'},'data':{'assets':uuids}}    
    return HttpResponse(json.dumps(response), mimetype='application/json')  
            
    