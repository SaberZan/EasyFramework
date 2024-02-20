import Koa from 'koa';
import KoaRange from 'koa-range';
import KoaStatic from 'koa-static';
import serve from 'koa-static';
import path from 'path'

let app = new Koa();

let args = process.argv;
let port = args[2] || 9999;
let updatePath = path.join(__dirname)+'/' + args[3] || 'public/Update'; ;
console.log("port == " + port + "updatePath == " + updatePath);
const home  = serve(updatePath);

app.use(KoaRange);
app.use(home);

app.listen(port);