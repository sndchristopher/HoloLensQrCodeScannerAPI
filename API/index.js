import express from 'express';
import Jimp from "jimp";
import qrCodeReader from "qrcode-reader";

const app = express();
app.use(express.json({ limit: "50mb" }));
app.use(express.urlencoded({ limit: '50mb' }));
const port = 3000;


app.get('/', (req, res) => {
    res.send('Hello World!')
})

app.post('/qr', async (req, res) => {
    let qrCodeInput = req.body.qrCode;

    if (qrCodeInput === undefined) {
        res.status(400).send('Error, qrCode not defined');
        return;
    }
    const qrCodeInputBuffer = Buffer.from(qrCodeInput);

    const qrCodeImage = await Jimp.read(qrCodeInputBuffer)
        .catch(err => {
            if (err) {
                console.log(err);
            }
        });

    const qrCodeInstance = new qrCodeReader();
    qrCodeInstance.callback = function (err, qrCodeValue) {
        if (err) {
            console.error(err);
            res.status(400).send("No QrCode found");
            return;
        }
    
            console.log(qrCodeValue.result);
            res.send(qrCodeValue.result);    
    };

    qrCodeInstance.decode(qrCodeImage.bitmap);
})

app.listen(port, () => {
    console.log(`Example app listening on port ${port}`)
})