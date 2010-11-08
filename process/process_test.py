import os
import time
import xmlrpclib
from ConfigParser import ConfigParser
proxy = xmlrpclib.ServerProxy("http://localhost:5060")
ppt2os_config = ConfigParser()
ppt2os_config.read(os.path.join('../','ppt2os.ini'))
print "\nTesting Converter...\n"
converter_proxy = xmlrpclib.ServerProxy("http://127.0.0.1:%s" % ppt2os_config.getint("converter","port"))
slides = converter_proxy.ppt2os.ppt_to_png('process_test.ppt','jpg')
print "slides converted: %s" % slides

print "\nTesting Uploader...\n"
firstname = raw_input('What is your avatar firstname? ')
lastname = raw_input('What is your avatar lastname? ')
password = raw_input('What is your avatar password? ')

upload_proxy = xmlrpclib.ServerProxy("http://127.0.0.1:%s" % ppt2os_config.getint("uploader","port"))
logged_in = upload_proxy.exposed.login(firstname,lastname,password,ppt2os_config.get("web","user_server"))
if logged_in == True:
    print "Authenticated with %s" % ppt2os_config.get("web","user_server")
    print "Uploading slides now..."
    while True:
        try:
            time.sleep(1)
            uuids = upload_proxy.exposed.upload_textures(slides)
            print "Uploaded to inventory: %s\n" % uuids
            break
        except xmlrpclib.Fault:
            pass
else:
    print "Authentication with %s failed!" % ppt2os_config.get("web","user_server")

