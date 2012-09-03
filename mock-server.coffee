net = require 'net'

host = 'transgame.csail.mit.edu'
port = 1101

net.createServer((sock) ->
  address = sock.remoteAddress
  console.log "CONNECTED: #{address}"
  interv = setInterval(() ->
    sock.write JSON.stringify({"corners":[0,0,0,0],"f0":[5888,2575,108],"f1":[3375,2694,-1],"f2":[0,0,0],"f3":[0,0,0],"f4":[0,0,0]}) + '\n'
  , 100)
  sock.on('close', (data) ->
    console.log "DISCONNECTED: #{address}"
    clearInterval(interv)
  )
).listen(port, host)
