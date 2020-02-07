FROM scottyhardy/docker-wine:stable-2.0.1

USER root
RUN addgroup --gid 111 apophysis && adduser --system --home /opt/apophysis --uid 111 --gid 111 --disabled-password --shell /bin/bash apophysis

USER apophysis
RUN wine wineboot --init && mkdir -p ~/drive_c/Apophysis

WORKDIR /opt/apophysis/drive_c/Apophysis
COPY target/apoc-release .

ENTRYPOINT [ "wineconsole", "--backend=curses", "apoc.exe" ]
