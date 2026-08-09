declare module 'qz-tray' {
  export interface QzPrinterConfig {
    getPrinter(): string | object;
  }

  export interface QzPrintData {
    type: 'raw';
    format: 'command';
    data: string;
  }

  export interface QzConnectionInfo {
    host?: string;
    port?: number;
    socket?: string;
    [key: string]: unknown;
  }

  export interface QzTray {
    websocket: {
      isActive(): boolean;
      connect(): Promise<void>;
      getConnectionInfo(): QzConnectionInfo;
      setClosedCallbacks(callback: (event: unknown) => void): void;
      setErrorCallbacks(callback: (event: unknown) => void): void;
    };
    printers: {
      find(query?: string): Promise<string | string[]>;
    };
    configs: {
      create(printer: string): QzPrinterConfig;
    };
    print(config: QzPrinterConfig, data: Array<QzPrintData | string>): Promise<void>;
    security?: {
      setCertificatePromise(handler: (resolve: (certificate: string) => void, reject: (erro: unknown) => void) => void): void;
      setSignaturePromise(handler: (toSign: string) => (resolve: (signature?: string) => void, reject: (erro: unknown) => void) => void): void;
    };
  }

  const qz: QzTray;
  export default qz;
}
